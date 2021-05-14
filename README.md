# River Monitor

Software supporting an artwork by [Aaron Marx](http://aaronmarx.com/) at the Minneapolis Public Service Building, 505 Fourth Avenue South, Minneapolis, Minnesota 55415

The artwork occupies the ceiling of an elevator lobby. It is constructed from wood slats, profiled to represent the topography of the Mississippi River as it flows through downtown Minneapolis. The slats are backlit with an array of LED lights which are animated using a [Color Kinetics](https://www.colorkinetics.com/) controller. The lighting animations are triggered, in real time, by the conditions of the Mississippi River at the Upper St. Anthony Falls Lock and Dam.

# Software Applications

The software applications in this reposity fall into 2 broad categories: data retrieval and lighting control.

# Data sources

Applications in this repository retrieve river flow data from USGS Water Services:

 - [Retrieve Sample](https://waterdata.usgs.gov/nwis/inventory/?site_no=05331000&agency_cd=USGS)
 - [Reference](https://waterservices.usgs.gov/)

# Event Data

In order to communicate with control devices, a small package of data (in JSON format) is assembled to represent the current measurement retrieved. The data looks like this:

`{ 
    "Site": "MISSISSIPPI RIVER AT ST. PAUL, MN"
    "Time": "3/26/2021 4:15:00 PM",
    "Temperature": 43.5,
    "DischargeVolume": 26700.0,
    "GageHeight": 5.55,
    "Elevation": 689.32,
    "Velocity": 2.48,
    "IntensityLevel": 5 
}`

# USGS Water Data

Real time Mississippi River data is retrieved from [USGS Water Services](https://waterservices.usgs.gov/). The reading of interest is discharge (parameter 00060), which is measured in cubic feet per second. The reading is produced by intruments at St. Paul (site 05331000). Many other parameters are available on the [physical parameters page](https://help.waterdata.usgs.gov/parameter_cd?group_cd=PHY).

The query for retrieving the most recent discharge reading in JSON format is:

[https://waterservices.usgs.gov/nwis/iv/?format=json&sites=05331000&parameterCd=00060&siteStatus=all](https://waterservices.usgs.gov/nwis/iv/?format=json&sites=05331000&parameterCd=00060&siteStatus=all)
