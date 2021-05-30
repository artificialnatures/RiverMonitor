#include <JsonParserGeneratorRK.h>
JsonParser parser;
SerialLogHandler logHandler;

enum class DeviceState
{
    Started,
    Waiting,
    MeasurementRequested,
    ReceivingMeasurement,
    MeasurementReceived,
    Testing
};

//EVENTS
const char * EventToggleTesting = "minneapolis-505FourthAveS-test";
const char * EventTriggerMeasurement = "minneapolis-505FourthAveS-discharge-measurement-trigger";
const char * EventMeasurementRequest = "mississippi-stpaul-discharge";
const char * EventMeasurementReceived = "hook-response/mississippi-stpaul-discharge";
const char * EventReport = "minneapolis-505FourthAveS-info";

//Discharge values for Mississippi River at St. Paul, MN in cubic feet per second
const int UpperDischargeBounds[] = 
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

enum class ColorKineticsRequest
{
    TurnLightsOff,
    SetIntensity,
    SetRelativeIntensity,
    SetShow
};

enum class ColorKineticsResponse
{
    ModeWasSet,
    LightsAreOff,
    IntensityWasSet,
    NothingWasSet,
    ShowWasSet,
    ErrorOccurred
};

namespace ColorKineticsCodes
{
    //Requests
    const String TurnLightsOff = "X01";
    const String SetIntensity = "X02";
    const String SetRelativeIntensity = "X03";
    const String SetShow = "X04";
    //Responses
    const String ModeWasSet = "Y00";
    const String LightsAreOff = "Y01";
    const String IntensityWasSet = "Y02";
    const String NothingWasSet = "Y03";
    const String ShowWasSet = "Y04";
    const String ErrorOccurred = "Y0F";
}

DeviceState state = DeviceState::Started;
const time_t SecondsBetweenRetrievals = 3600;
const time_t SecondsBeforeRequestFails = 30;
const time_t SecondsBetweenTestCycles = 30;
time_t timeAtRequest = 0;
time_t timeAtLastSuccessfulRetrieval = 0;
time_t timeAtLastTestCycle = 0;
int dischargeMeasurement = 0;
int lightingShow = 0;

void setup() 
{
    InitializeSerialConnection();
    Subscribe(EventToggleTesting, ToggleTesting);
    Subscribe(EventTriggerMeasurement, TriggerRequest);
    Subscribe(EventMeasurementReceived, ReceiveMessagePacket);
}

void loop() 
{
    switch (state)
    {
    case DeviceState::Started:
        state = DeviceState::Waiting;
        break;
    case DeviceState::Waiting:
        if (Time.now() > timeAtLastSuccessfulRetrieval + SecondsBetweenRetrievals)
        {
            RequestMeasurement();
        }
        break;
    case DeviceState::MeasurementRequested:
        CheckMeasurementTimeout();
        break;
    case DeviceState::ReceivingMeasurement:
        CheckMeasurementTimeout();
        break;
    case DeviceState::MeasurementReceived:
        UpdateLightingShow();
        break;
    case DeviceState::Testing:
        CycleTestShow();
        break;
    }
    delay(1000); //Loop every second
}

void InitializeSerialConnection()
{
    //ColorKinetics: 9600 baud, 8 data bits, no parity, 1 stop bit, no flow control
    Serial1.begin(9600, SERIAL_8N1);
}

void SendSerialCommand(String command, int value)
{
    //convert to hex and append to command
    char hexValue[3] = {'0', '0', 0};
    sprintf(hexValue, "%X", value);
    String serialCommand = String::format("%s%s", command, hexValue);
    Log.info("Sending serial command: %s", serialCommand);
    //Serial1.write(serialCommand);
    //String serialResponse = Serial1.readString();
    //Log.info("Serial response: %s", serialResponse);
}

void Subscribe(const char * eventName, EventHandler handler)
{
    Particle.subscribe(eventName, handler, MY_DEVICES);
    Log.info("Subscribed to event: %s", eventName);
}

void Publish(const char * eventName, const char * eventData)
{
    Particle.publish(eventName, eventData, MY_DEVICES);
    Log.info("Published event: %s \nwith data: %s", eventName, eventData);
}

//callback for event: minneapolis-505FourthAveS-test
void ToggleTesting(const char *event, const char *data)
{
    if (state == DeviceState::Testing)
    {
        state = DeviceState::Waiting;
        //TODO: tear down testing
    }
    else
    {
        state = DeviceState::Testing;
        //TODO: initialize testing
    }
}

void CycleTestShow()
{
    if (Time.now() > timeAtLastTestCycle + SecondsBetweenTestCycles)
    {
        lightingShow = lightingShow + 1;
        if (lightingShow > arraySize(UpperDischargeBounds))
        {
            lightingShow = 1;
        }
        Log.info("Testing lighting show %d.", lightingShow);
        timeAtLastTestCycle = Time.now();
    }
}

//callback for event: minneapolis-505FourthAveS-discharge-measurement-trigger
void TriggerRequest(const char *event, const char *data)
{
    RequestMeasurement();
}

void RequestMeasurement()
{
    if (state == DeviceState::Waiting)
    {
        parser.clear();
        timeAtRequest = Time.now();
        Publish(EventMeasurementRequest, ""); //start a request via Particle webhook to retrieve new USGS Water Service data
        state = DeviceState::MeasurementRequested;
    }
}

//callback for event: hook-response/mississippi-stpaul-discharge
void ReceiveMessagePacket(const char *event, const char *data)
{
    Log.info("Received measurement packet from event: %s\n%s\n", event, data);
    parser.addChunkedData(event, data);
    if (parser.parse())
    {
        Log.info("Received all parts of measurement data.");
        /* Parse the JSON response from USGS Water Services
         * See an example: https://waterservices.usgs.gov/nwis/iv/?format=json&sites=05331000&parameterCd=00060&siteStatus=all
         */
        String measurementText = parser.getReference().key("value").key("timeSeries").index(0).key("values").index(0).key("value").index(0).key("value").valueString();
        dischargeMeasurement = measurementText.toInt();
        //Publish(EventReport, String::format("Retrieved discharge measurement of: %d", measurement));
        Log.info("Retrieved discharge measurement of: %d", dischargeMeasurement);
        state = DeviceState::MeasurementReceived;
        timeAtLastSuccessfulRetrieval = Time.now();
    }
    else
    {
        Log.info("Waiting for the rest of the measurement data.");
        state = DeviceState::ReceivingMeasurement;
    }
}

void CheckMeasurementTimeout()
{
    bool hasTimedOut = (state == DeviceState::ReceivingMeasurement 
                        || state == DeviceState::MeasurementRequested)
                        && Time.now() - timeAtRequest > SecondsBeforeRequestFails;
        
    if (hasTimedOut)
    {
        //Publish(EventReport, "Measurement request timed out.");
        Log.info("Measurement request timed out.");
        state = DeviceState::Waiting;
    }
}

int FindLightingShow(int dischargeMeasurement)
{
    int boundsLength = arraySize(UpperDischargeBounds);
    for (int index = 0; index < boundsLength; index++) 
    {
        if (dischargeMeasurement < UpperDischargeBounds[index])
        {
            return index + 1;
        }
    }
    return 1;
}

void UpdateLightingShow()
{
    int show = FindLightingShow(dischargeMeasurement);
    if (lightingShow != show)
    {
        lightingShow = show;
        Log.info("Set lighting show to: %d", lightingShow);
        SendSerialCommand(ColorKineticsCodes::SetShow, lightingShow);
    }
}