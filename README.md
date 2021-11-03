# River Monitor

Software supporting an artwork by [Aaron Marx](http://aaronmarx.com/) at the Minneapolis Public Service Building, 505 Fourth Avenue South, Minneapolis, Minnesota 55415

The artwork occupies the ceiling of an elevator lobby. It is constructed from wood slats, profiled to represent the topography of the Mississippi River as it flows through downtown Minneapolis. The slats are backlit with an array of LED lights which are animated using a [Color Kinetics](https://www.colorkinetics.com/) controller. The lighting animations are triggered, in real time, by the conditions of the Mississippi River.

# Microcontroller

The device that retrieves data from USGS Water Services and sends commands to the lighting controller is a [Raspberry Pi 3b](https://www.raspberrypi.org/) microcontroller. It is running the [Ubuntu Core 20](https://ubuntu.com/core) operating system. This device connects to the internet via a WIFI network.

## Software

The microcontroller is running an program written in the F# programming language using the [.NET](https://dotnet.microsoft.com/) software development platform. The source can be viewed on [GitHub](https://github.com/artificialnatures/RiverMonitor).

Build the console application using the [build.sh](build.sh) script on a system with a BASH shell and .NET 5 installed. Copy the built application from the build folder onto the Ubuntu server at `/home/ubuntu/rivermonitor`. Copy the [rivermonitor.service](rivermonitor.service) file onto the Ubuntu server at `/etc/systemd/system/rivermonitor.service`. Start the service by issuing the command:

`systemctl start rivermonitor`

and ensure the service starts automatically on boot using the command:

`systemctl enable rivermonitor`

Overwrite the `settings.json` file in the application folder with `test.json` to test the system and `live.json` to retrieve live measurements from USGS.

# USGS Water Data

Real time Mississippi River data is retrieved from [USGS Water Services](https://waterservices.usgs.gov/). The reading of interest is discharge (parameter 00060), which is measured in cubic feet per second. The reading is produced by instruments at St. Paul (site 05331000). Many other parameters are available on the [physical parameters page](https://help.waterdata.usgs.gov/parameter_cd?group_cd=PHY).

The query for retrieving the most recent discharge reading in JSON format is:

[https://waterservices.usgs.gov/nwis/iv/?format=json&sites=05331000&parameterCd=00060&siteStatus=all](https://waterservices.usgs.gov/nwis/iv/?format=json&sites=05331000&parameterCd=00060&siteStatus=all)
