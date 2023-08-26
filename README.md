# Co2.Monitor

`ZyAura ZG-01` sensor based family modules.

![Generic Device Look](https://user-images.githubusercontent.com/22738239/73683926-7a247c80-46c3-11ea-99cb-a086262aa693.jpg)

- Product: USB-zyTemp
- Manufacturer: Holtek
- SerialNumber: 2.00
- VID: 0x04D9
- PID: 0xA052

## Programming with HidSharp

![Console Application Log](https://user-images.githubusercontent.com/11328666/263465860-6dd01731-dba8-4fe9-9ae6-c22ed64ec415.jpg)

Find and try to get a device with pair of vid/pid:

```csharp
HidDevice? hid =
    DeviceList.Local
    .GetHidDevices( VID, PID )
    .FirstOrDefault();
```

Open hid stream, then set feature flags with zero filled array `magic_table`:

```csharp
using var hidStream = hid.Open();

byte[] magic_table = new byte[9];
hidStream.SetFeature( magic_table );
```

We are ready to read (poll) data packets:

```csharp
var raw = hidStream.Read()[1..];
```

## Related Projects

- [CLI for MasterKit CO2 Monitor](https://github.com/dmage/co2mon)
- [CO2 meter](https://github.com/vfilimonov/co2meter). A Python library for USB CO2 meter

## Additional Reading List

- [CO2 Meter Hacking](https://revspace.nl/CO2MeterHacking). Protocol reversing and description.
- [Reverse-Engineering a low-cost USB CO₂ monitor](https://hackaday.io/project/5301-reverse-engineering-a-low-cost-usb-co-monitor). I'm trying to get data out of a relatively low-cost (80€) CO₂ monitor that appears to have a USB connection for data as well as for power. by Henryk Plötz
- [USB Device Tree Viewer V3.8.8](https://www.uwe-sieber.de/usbtreeview_e.html)
- `ru` [Forewarned is forearmed. Part 3](https://habr.com/ru/companies/masterkit/articles/248403/). by MasterKit (device rebrander)
- [AN146-RAD-0401-serial-communication](http://co2meters.com/Documentation/AppNotes/AN146-RAD-0401-serial-communication.pdf). RS-232 serial protocol description
- [Reading a zyTemp Carbon Dioxide Monitor using Tkinter on Linux](https://blog.tfiu.de/reading-a-zytemp-carbon-dioxide-monitor-using-tkinter-on-linux.html)

## Protocol Analysis

### Unique Packet Ids

- Polling: 6D, 6E, 71, 50, 57, 56, 41, 43, 42, 4F, 52
- Startup: 63, 64, 28, 29, 26, 25, 71, 45, 79, 75, 2E, 41, 47, 49, 72, 69, 4B, 51, 6F, 6C, 76, 77, 7A, 78, 53, 3D, 60, 54, 7B, 55, 4E, 3A, 58, 70, 73, 68, 2F, 4C, 3B, 5D, 5E, 66, 4D, 5B, 59, 65, 6E, 5C, 74

### Startup Data Snapshot

```plain
281563A00D000000        5475
291649880D000000        5705
261CD7190D000000        7383
252990DE0D000000        10640
710000710D000000        0
457FFFC30D000000        32767
79FEE75E0D000000        65255
751220A70D000000        4640
2E04B0E20D000000        1200
414380040D000000        17280
470376C00D000000        886
490A6ABD0D000000        2666
724C26E40D000000        19494
6919AA2C0D000000        6570
4B4DF78F0D000000        19959
510201540D000000        513
6FC350820D000000        50000
6C0835A90D000000        2101
766014EA0D000000        24596
77128E170D000000        4750
7A0FD55E0D000000        4053
780000780D000000        0
532666DF0D000000        9830
3D8000BD0D000000        32768
603DF3900D000000        15859
54224DC30D000000        8781
7B0A3DC20D000000        2621
5500BE130D000000        190
4E0D87E20D000000        3463
3A04013F0D000000        1025
58635F1A0D000000        25439
700BB8330D000000        3000
730A05820D000000        2565
6803208B0D000000        800
2F1333750D000000        4915
4C00004C0D000000        0
3B0190CC0D000000        400
5D00005D0D000000        0
5EFFF3500D000000        65523
660000660D000000        0
4D0064B10D000000        100
5B0190EC0D000000        400
5901F24C0D000000        498
650190F60D000000        400
6E001E8C0D000000        30
5C07D0330D000000        2000
749EFF110D000000        40703
```
