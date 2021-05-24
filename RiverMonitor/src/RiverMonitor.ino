#include <JsonParserGeneratorRK.h>
JsonParser parser;

enum class DeviceState
{
    Started,
    Waiting,
    MeasurementRequested,
    ReceivingMeasurement,
    MeasurementReceived,
    Testing
};

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

#define SerialBaudRate 9600
#define SerialDataBits 8
#define SerialParity false
#define SerialStopBits 1
#define SerialFlowControl false

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
int lightingShow = 0;

void setup() 
{
    Particle.subscribe("minneapolis-505FourthAveS-test", toggleTesting, MY_DEVICES);
    Particle.subscribe("minneapolis-505FourthAveS-discharge-measurement-trigger", triggerRequest, MY_DEVICES);
    Particle.subscribe("hook-response/mississippi-stpaul-discharge", receiveMessagePacket, MY_DEVICES);
}

void loop() 
{
    switch (state)
    {
    case DeviceState::Started:
        state = DeviceState::Waiting;
        Particle.publish("minneapolis-505FourthAveS-info", "Started.", PRIVATE);
        break;
    case DeviceState::Waiting:
        if (Time.now() > timeAtLastSuccessfulRetrieval + SecondsBetweenRetrievals)
        {
            RequestMeasurement();
        }
        break;
    case DeviceState::MeasurementRequested:
        break;
    case DeviceState::ReceivingMeasurement:
        break;
    case DeviceState::MeasurementReceived:
        timeAtLastSuccessfulRetrieval = Time.now();
        UpdateLightingState();
        break;
    case DeviceState::Testing:
        CycleTestShow();
        break;
    }
    delay(1000); //Loop every second
}

//callback for event: minneapolis-505FourthAveS-test
void toggleTesting(const char *event, const char *data)
{
    if (state == DeviceState::Testing)
    {
        state = DeviceState::Waiting;
    }
    else
    {
        state = DeviceState::Testing;
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
        SetLightingShow();
        timeAtLastTestCycle = Time.now();
    }
}

//callback for event: minneapolis-505FourthAveS-discharge-measurement-trigger
void triggerRequest(const char *event, const char *data)
{
    RequestMeasurement();
}

//callback for event: hook-response/mississippi-stpaul-discharge
void receiveMessagePacket(const char *event, const char *data)
{
    parser.addChunkedData(event, data);
    if (parser.parse())
    {
        state = DeviceState::MeasurementReceived;
        timeAtLastSuccessfulRetrieval = Time.now();
    }
    else
    {
        state = DeviceState::ReceivingMeasurement;
    }
    if (Time.now() - timeAtRequest > SecondsBeforeRequestFails)
    {
        Particle.publish("minneapolis-505FourthAveS-info", "Failed to retrieve discharge measurement.", PRIVATE);
        state = DeviceState::Waiting;
    }
}

void RequestMeasurement()
{
    if (state == DeviceState::Waiting)
    {
        parser.clear();
        timeAtRequest = Time.now();
        Particle.publish("hook-response/mississippi-stpaul-discharge", "", PRIVATE); //start a request via Particl webhook to retrieve new USGS Water Service data
        state = DeviceState::MeasurementRequested;
    }
}

void UpdateLightingState()
{
    /* Parse the JSON response from USGS Water Services
     * See an example: https://waterservices.usgs.gov/nwis/iv/?format=json&sites=05331000&parameterCd=00060&siteStatus=all
     */
    String measurementText = parser.getReference().key("value").key("timeSeries").index(0).key("values").index(0).key("value").index(0).key("value").valueString();
    int measurement = measurementText.toInt();
    Particle.publish("minneapolis-505FourthAveS-info", String::format("Retrieved discharge measurement of: %d", measurement), PRIVATE);
    int show = FindLightingShow(measurement);
    if (lightingShow != show)
    {
        lightingShow = show;
        SetLightingShow();
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

void SetLightingShow()
{
    //TODO: set show on color kinetics...
    Particle.publish("minneapolis-505FourthAveS-info", String::format("Set lighting show to: %d", lightingShow));
}