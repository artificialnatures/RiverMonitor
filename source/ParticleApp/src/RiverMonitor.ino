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
int lightProgram = 1; //The program that has been sent to the lighting controller (1 through 10)
time_t timeAtLastSuccessfulRetrieval = 0;
time_t timeAtPreviousRequest = 0;
time_t secondsBetweenRetrievals = 60;
time_t secondsBeforeRequestFails = 10;
bool measurementHasBeenRequested = false;

void setup() 
{
    #if defined(DEBUG_BUILD)
    Mesh.off();
    BLE.off();
    #endif
    Particle.subscribe("hook-response/mississippi-stpaul-discharge", receivedRiverMeasurement, MY_DEVICES); //Call the receivedRiverMeasurement function when data is received
    RGB.control(true); //Use the onboard LED to signal status
    RGB.color(255, 0, 0); //Display red to indicate that the board is initializing
}

void requestRiverMeasurement()
{
    /* Request the most recent river measurement from the mississippi-stpaul-discharge webhook
     * Defined in the Particle web console: https://console.particle.io/
     * Retrieves data from USGS Water Services, e.g. https://waterservices.usgs.gov/nwis/iv/?format=json&sites=05331000&parameterCd=00060&siteStatus=all
     */
    Particle.publish("mississippi-stpaul-discharge", "", PRIVATE);
    RGB.color(0, 128, 128); //indicate that a request has been made
    timeAtPreviousRequest = Time.now();
    measurementHasBeenRequested = true;
}

String extractDischargeMeasurement(const char *data)
{
    String dischargeMeasurement = "No measurement found.";
    /* Parse the JSON response from USGS Water Services
     * See an example: https://waterservices.usgs.gov/nwis/iv/?format=json&sites=05331000&parameterCd=00060&siteStatus=all
     */
    JSONValue json = JSONValue::parseCopy(data);
    JSONObjectIterator topLevelIterator(json);
    while (topLevelIterator.next())
    {
        if (topLevelIterator.name() == "value")
        {
            JSONObjectIterator valueIterator(topLevelIterator.value());
            while (valueIterator.next())
            {
                if (valueIterator.name() == "timeSeries")
                {
                    JSONArrayIterator seriesArrayIterator(valueIterator.value());
                    if (seriesArrayIterator.next()) //only take the first item
                    {
                        JSONObjectIterator timeSeriesItemIterator(seriesArrayIterator.value());
                        while (timeSeriesItemIterator.next())
                        {
                            if (timeSeriesItemIterator.name() == "values")
                            {
                                JSONArrayIterator measurementArrayIterator(timeSeriesItemIterator.value());
                                if (measurementArrayIterator.next()) //only take the first item
                                {
                                    JSONObjectIterator measurementItemIterator(measurementArrayIterator.value());
                                    while (measurementItemIterator.next())
                                    {
                                        if (measurementItemIterator.name() == "value")
                                        {
                                            JSONArrayIterator valueArrayIterator(measurementItemIterator.value());
                                            if (valueArrayIterator.next()) //only take the first item
                                            {
                                                JSONObjectIterator valueItemIterator(valueArrayIterator.value());
                                                while (valueItemIterator.next())
                                                {
                                                    if (valueItemIterator.name() == "value")
                                                    {
                                                        dischargeMeasurement = valueItemIterator.value(); //this is the actual discharge measurement
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    return dischargeMeasurement;
}

void receivedRiverMeasurement(const char *event, const char *data) 
{
    String dischargeMeasurement = extractDischargeMeasurement(data);
    Particle.publish("minneapolis-505FourthAveS-discharge-measurement-retrieved", dischargeMeasurement); //record that a measurement was retrieved
    RGB.color(0, 255, 0); //indicate that the measurement was retrieved
    timeAtLastSuccessfulRetrieval = Time.now();
    measurementHasBeenRequested = false;
}

void loop() 
{
    bool previousRequestFailed = measurementHasBeenRequested && Time.now() - timeAtPreviousRequest > secondsBeforeRequestFails;
    bool shouldMakeNextRequest = Time.now() - timeAtLastSuccessfulRetrieval > secondsBetweenRetrievals;
    if (previousRequestFailed || shouldMakeNextRequest)
    {
        requestRiverMeasurement();
    }
    delay(1000); //Loop every second
}