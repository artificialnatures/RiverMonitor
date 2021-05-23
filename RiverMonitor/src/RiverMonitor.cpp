/******************************************************/
//       THIS IS A GENERATED FILE - DO NOT EDIT       //
/******************************************************/

#include "Particle.h"
#line 1 "/Users/aaron/GitHub/artificialnatures/RiverMonitor/RiverMonitor/src/RiverMonitor.ino"
#include <JsonParserGeneratorRK.h>
int findLightingProgram(int dischargeMeasurement);
void setup();
bool shouldRequestMeasurement();
void requestRiverMeasurement();
const char * extractDischargeMeasurement(const char *data);
void receivedRiverMeasurement(const char *event, const char *data);
void loop();
#line 2 "/Users/aaron/GitHub/artificialnatures/RiverMonitor/RiverMonitor/src/RiverMonitor.ino"
JsonParser jsonParser;
//Discharge values for Mississippi River at St. Paul, MN in cubic feet per second
int upperDischargeBounds[] = 
{
    7000,    //Level 1
    10000,   //Level 2
    15000,   //Level 3
    20000,   //Level 4
    30000,   //Level 5
    40000,   //Level 6
    50000,   //Level 7
    60000,   //Level 8
    75000,   //Level 9
    1000000  //Level 10
};
int lightingProgram = 1; //The program that has been sent to the lighting controller (1 through 10)

int findLightingProgram(int dischargeMeasurement)
{
    for (int index = 0; index < arraySize(upperDischargeBounds); index++) 
    {
        if (dischargeMeasurement < upperDischargeBounds[index])
        {
            return index + 1;
        }
    }
    return 1;
}

time_t timeAtLastSuccessfulRetrieval = 0;
time_t timeAtPreviousRequest = 0;
time_t secondsBetweenRetrievals = 60;
time_t secondsBeforeRequestFails = 20;
bool measurementHasBeenRequested = false;

void setup() 
{
    Particle.subscribe("hook-response/mississippi-stpaul-discharge", receivedRiverMeasurement, MY_DEVICES); //Call the receivedRiverMeasurement function when data is received
    RGB.control(true); //Use the onboard LED to signal status
    RGB.color(255, 0, 0); //Display red to indicate that the board is initializing
}

bool shouldRequestMeasurement()
{
    bool previousRequestFailed = measurementHasBeenRequested && Time.now() - timeAtPreviousRequest > secondsBeforeRequestFails;
    bool shouldMakeNextRequest = Time.now() - timeAtLastSuccessfulRetrieval > secondsBetweenRetrievals;
    return previousRequestFailed || shouldMakeNextRequest;
}

void requestRiverMeasurement()
{
    /* Request the most recent river measurement from the mississippi-stpaul-discharge webhook
     * Defined in the Particle web console: https://console.particle.io/
     * Retrieves data from USGS Water Services, e.g. https://waterservices.usgs.gov/nwis/iv/?format=json&sites=05331000&parameterCd=00060&siteStatus=all
     */
    jsonParser.clear(); //clear out previously recieved messages
    measurementHasBeenRequested = true;
    timeAtPreviousRequest = Time.now();
    Particle.publish("mississippi-stpaul-discharge", "", PRIVATE);
    RGB.color(0, 128, 128); //indicate that a request has been made
}

const char * extractDischargeMeasurement(const char *data)
{
    /* Parse the JSON response from USGS Water Services
     * See an example: https://waterservices.usgs.gov/nwis/iv/?format=json&sites=05331000&parameterCd=00060&siteStatus=all
     */
    return jsonParser.getReference().key("value").key("timeSeries").index(0).key("values").index(0).key("value").index(0).key("value").valueString();
}

void receivedRiverMeasurement(const char *event, const char *data) 
{
    //Handle multi-part responses. Response packets from Particle webhooks are limited to 512 bytes.
	jsonParser.addString(data);
	if (jsonParser.parse()) 
	{
		//All parts have been received
		String dischargeMeasurementText = extractDischargeMeasurement(data);
        Particle.publish("minneapolis-505FourthAveS-discharge-measurement-retrieved", String::format("Retrieved discharge measurement of: %s", dischargeMeasurementText)); //record that a measurement was retrieved
        int dischargeMeasurement = dischargeMeasurementText.toInt();
        if (dischargeMeasurement != 0)
        {
            //TODO: Determine which level and  program to set
            RGB.color(0, 255, 0); //indicate that the measurement was retrieved
            timeAtLastSuccessfulRetrieval = Time.now();
            measurementHasBeenRequested = false;
            int matchingLightingProgram = findLightingProgram(dischargeMeasurement);
            if (matchingLightingProgram != lightingProgram)
            {
                lightingProgram = matchingLightingProgram;
                Particle.publish("minneapolis-505FourthAveS-set-lighting-program", String::format("Setting lighting program to: %d", lightingProgram));
            }
        }
	}
}

void loop() 
{
    if (shouldRequestMeasurement())
    {
        requestRiverMeasurement();
    }
    delay(1000); //Loop every second
}