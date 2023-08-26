using HidSharp;
using System.Text;

const int VID = 0x04d9;
const int PID = 0xa052;

HidDevice? hid = null;

Console.Write( "Press any key to close the app." );
Console.Write( "Looking for USB device." );
while (hid is null && !Console.KeyAvailable) {
    hid =
        DeviceList.Local
        .GetHidDevices( VID, PID )
        .FirstOrDefault();

    if (hid is not null) break;

    Console.Write( "." );
    Thread.Sleep( 1000 );
}

if (hid is null) {
    Console.WriteLine( "\nNot found." );
    return; // Close the application
}

Console.WriteLine( hid.ReleaseNumber );
bool decode = hid.ReleaseNumber.Major < 2;

using var hidStream = hid.Open();

byte[] magic_table = new byte[9];
hidStream.SetFeature( magic_table );

Thread.Sleep( 200 );

HashSet<byte> codes = new();

while (!Console.KeyAvailable) {
    try {
        var raw = hidStream.Read()[1..];
        var data =
            decode ? Decode( raw )
            : raw;

        byte calcCrc = Crc( data[0..3] );
        byte crc = data[3];
        if (calcCrc != crc)
            Console.WriteLine( $"[!] Invalid CRC --> act:{calcCrc} <> exp:{crc}."  );

        codes.Add( data[0] );

        int value = (data[1] << 8) | data[2];

        double tempc = 20.0;
        if (data[0] == 0x42)
            tempc = value / 16.0 - 273.15;

        string ss =
            data[0] switch {
                0x42 => $"\t> {tempc:F2} °C", // temperature / {CToF( tempc ):F2} °F
                0x50 => $"\t> {value} ppm", // CO2
                //0x41 => $"\t{value / 100.0:F2} %", // relative humidity
                0x52 => $"\t> {value / 16.0 - tempc:F0} mmHg :{value}", // lower pressure?
                //0x56 => $"\t{value / 16.0 - tempc:F0} mmHg :{value}", // higher pressure?
                _ => $"\t{value}",
            };
        Console.WriteLine( Convert.ToHexString( data ) + ss );
    }
    catch (IOException e) { // disconnected
        Console.WriteLine( e );
        break;
    }
    //Thread.Sleep( 5000 );
}

Console.WriteLine(
    string.Join(
        ", ",
        Convert.ToHexString( codes.ToArray() )
            .Chunk( 2 )
            .Select( chs => new string( chs ) ))
    );

Console.WriteLine( "EOF" );

byte Crc( byte[] data )
    => (byte)((data[0] + data[1] + data[2]) & 0xFF);

double CelsiusToFahrenheit( double cels )
    => cels * 9.0 / 5.0 + 32.0;

byte[] Decode( byte[] buf )
{
    (buf[0], buf[2]) = (buf[2], buf[0]);
    (buf[1], buf[4]) = (buf[4], buf[1]);
    (buf[3], buf[7]) = (buf[7], buf[3]);
    (buf[5], buf[6]) = (buf[6], buf[5]);

    for (int i = 0; i < 8; ++i) {
        buf[i] ^= magic_table[i];
    }

    byte[] result = new byte[8];

    byte tmp = (byte)(buf[7] << 5);
    result[7] = (byte)((buf[6] << 5) | (buf[7] >> 3));
    result[6] = (byte)((buf[5] << 5) | (buf[6] >> 3));
    result[5] = (byte)((buf[4] << 5) | (buf[5] >> 3));
    result[4] = (byte)((buf[3] << 5) | (buf[4] >> 3));
    result[3] = (byte)((buf[2] << 5) | (buf[3] >> 3));
    result[2] = (byte)((buf[1] << 5) | (buf[2] >> 3));
    result[1] = (byte)((buf[0] << 5) | (buf[1] >> 3));
    result[0] = (byte)(tmp | (buf[0] >> 3));

    byte[] magic_word = Encoding.ASCII.GetBytes( "Htemp99e" );
    for (int i = 0; i < 8; ++i)
        result[i] -= (byte)((magic_word[i] << 4) | (magic_word[i] >> 4));

    return result;
}
