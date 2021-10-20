# River Monitor

Software supporting an artwork by [Aaron Marx](http://aaronmarx.com/) at the Minneapolis Public Service Building, 505 Fourth Avenue South, Minneapolis, Minnesota 55415

The artwork occupies the ceiling of an elevator lobby. It is constructed from wood slats, profiled to represent the topography of the Mississippi River as it flows through downtown Minneapolis. The slats are backlit with an array of LED lights which are animated using a [Color Kinetics](https://www.colorkinetics.com/) controller. The lighting animations are triggered, in real time, by the conditions of the Mississippi River.

# Particle Microcontroller

The device that retrieves data from USGS Water Services and sends commands to the lighting controller is a [Particle](https://www.particle.io/) [Boron](https://docs.particle.io/quickstart/boron/) microcontroller. This device can connect to the internet via 4G cellular networks. The device can be controlled and updated via the [Particle Console](https://console.particle.io/).

## Events

On the events page on the [Particle Console](https://console.particle.io/) you can see realtime updates that the microcontroller is sending about what it's doing. You can also send events to the microcontroller to trigger different actions.

### Data Events

 - `mississippi-stpaul-discharge`
   The microcontroller sends this event to the cloud to request updated USGS Water Services data. Responses are received via the `hook-response/mississippi-stpaul-discharge` event, as 512 byte parts that are recombined into a complete USGS Water Services measurement. The data retrieval works via a Particle webhook, which is defined on the integrations page of the [Particle Console](https://console.particle.io/).

### Informative Events

 - `minneapolis-505FourthAveS-info`
   Used by the microcontroller to send messages to the Particle Cloud about what it's doing. Data is sent as text.

### Trigger Events

You can use these events to trigger actions by the microcontroller. On the events page of the [Particle Console](https://console.particle.io/), select `Publish` and paste in the event name along with any data (if needed).

 - `minneapolis-505FourthAveS-test`
   No data needed. Toggle test mode on and off. Test mode continuously cycles through each lighting show at 30 second intervals.
 - `minneapolis-505FourthAveS-discharge-measurement-trigger`
   No data needed. Trigger retrieval of updated USGS Water Services data. Normally the program will request updated data once per hour.
 - `minneapolis-505FourthAveS-lighting-command`
   Data should be in the form Command:Value (see below for details). Trigger the microcontroller to send a command to the Color Kinetics lighting controller. The commands available are:
   
   - `TurnLightsOff:0` 
     Turns the lights off completely. 0 is the only accepted value. Equivalent to `SetIntensity:0`.
   - `SetIntensity:X`
     Sets the brightness of the lights. Accepted values are 0 (lights off) to 255 (full brightness).
   - `SetRelativeIntensity:X`
     Changes the brightness of the lights an increment. Accepted values are 0 (no change) to 255 (also no change). Values below 128 will increase the brightness, values above 128 will decrease the brightness by 255 - X. For example, `SetRelativeIntensity:25` would increase brightness by about 10%, while 230 would decrease the brightness by about 10%.
   - `SetShow:X`
     Sets the active lighting show. Accepted values are 1 (calmest show) to 10 (most active show).

## Code

On the code page of the [Particle Console](https://console.particle.io/) you can update the code for the program running on the microcontroller. This code should be treated as the authoritative source for the program running on the microcontroller. Select the `RiverMonitor` app to make changes to the program. Select `Save` and `Verify` to save your work and check that it compiles successfully. Select `Flash` to update the program running on the microcontroller.

An archival copy of the code is also kept in the Particle Code folder in this Google Drive folder. Previous code for other microcontroller platforms can be found in the Obsolete Experiments folder.

# USGS Water Data

Real time Mississippi River data is retrieved from [USGS Water Services](https://waterservices.usgs.gov/). The reading of interest is discharge (parameter 00060), which is measured in cubic feet per second. The reading is produced by instruments at St. Paul (site 05331000). Many other parameters are available on the [physical parameters page](https://help.waterdata.usgs.gov/parameter_cd?group_cd=PHY).

The query for retrieving the most recent discharge reading in JSON format is:

[https://waterservices.usgs.gov/nwis/iv/?format=json&sites=05331000&parameterCd=00060&siteStatus=all](https://waterservices.usgs.gov/nwis/iv/?format=json&sites=05331000&parameterCd=00060&siteStatus=all)
