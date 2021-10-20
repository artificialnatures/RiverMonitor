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
const char * EventLightingCommand = "minneapolis-505FourthAveS-lighting-command";
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

//ColorKinetics Requests
#define TurnLightsOff 0
#define SetIntensity 1
#define SetRelativeIntensity 2
#define SetShow 3
//ColorKinetics Responses
#define ModeWasSet 4
#define LightsAreOff 5
#define IntensityWasSet 6
#define NothingWasSet 7
#define ShowWasSet 8
#define ErrorOccurred 9

const char * ColorKineticsCodeNames[10] =
{
    //Requests
    "TurnLightsOff",
    "SetIntensity",
    "SetRelativeIntensity",
    "SetShow",
    //Responses
    "ModeWasSet",
    "LightsAreOff",
    "IntensityWasSet",
    "NothingWasSet",
    "ShowWasSet",
    "ErrorOccurred"
};

const char * ColorKineticsCodes[10] =
{
    //Requests
    "X01", //TurnLightsOff
    "X02", //SetIntensity
    "X03", //SetRelativeIntensity
    "X04", //SetShow
    //Responses
    "Y00", //ModeWasSet
    "Y01", //LightsAreOff
    "Y02", //IntensityWasSet
    "Y03", //NothingWasSet
    "Y04", //ShowWasSet
    "Y0F" //ErrorOccurred
};

DeviceState state = DeviceState::Started;
const time_t SecondsBetweenRetrievals = 3600;
const time_t SecondsBeforeRequestFails = 30;
const time_t SecondsBetweenTestCycles = 30;
time_t timeAtRequest = 0;
time_t timeAtLastSuccessfulRetrieval = 0;
time_t timeAtLastTestCycle = 0;
int dischargeMeasurement = 0;
int lightingShow = 0;

const String testCodes[10] = 
{
    "X0401",
    "X0402",
    "X0403",
    "X0404",
    "X0405",
    "X0406",
    "X0407",
    "X0408",
    "X0409",
    "X0410"
};
uint testCodeIndex = 0;

void setup() 
{
    InitializeSerialConnection();
    //Subscribe(EventToggleTesting, ToggleTesting);
    //Subscribe(EventTriggerMeasurement, TriggerRequest);
    //Subscribe(EventMeasurementReceived, ReceiveMessagePacket);
    //Subscribe(EventLightingCommand, TriggerLightingCommand);
}

void loop() 
{
    /*
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
    */
    String testCode = testCodes[testCodeIndex];
    Log.info(String::format("Sending code: %s", testCode));
    Serial1.write(testCode.c_str());
    Serial1.flush(); //ensure serialCommand has been sent to ColorKinetics device
    Log.info(String::format("Raw Response: %s", Serial1.readString()));
    if (testCodeIndex < 9)
    {
        testCodeIndex++;
    }
    else
    {
        testCodeIndex = 0;
    }
    delay(15000);
}

void TestCommandEncodingAndParsing()
{
    int values[5] = {0, 10, 105, 220, 255}; //00, 0A, 69, DC, FF
    for (int command = 0; command < 10; command++)
    {
        Log.info("Testing command %s", ColorKineticsCodeNames[command]);
        for (int valueIndex = 0; valueIndex < 5; valueIndex++)
        {
            String serialCommand = EncodeSerialCommand(command, values[valueIndex]);
            Log.info("Encoded command [%s, %d] as: %s", ColorKineticsCodeNames[command], values[valueIndex], serialCommand.c_str());
            PrintSerialCommand(serialCommand);
        }
    }
}

void InitializeSerialConnection()
{
    //ColorKinetics: 9600 baud, 8 data bits, no parity, 1 stop bit, no flow control
    Serial1.begin(9600, SERIAL_8N1);
    Serial1.setTimeout(1000); //try to receive data for 1 seconds
}

const String EncodeSerialCommand(int command, int value)
{
    char serialCommand[5];
    sprintf(serialCommand, "%s%02X", ColorKineticsCodes[command], value);
    Log.info("Encoded command [%s, %d] as: %s", ColorKineticsCodeNames[command], value, serialCommand);
    return String::format("%s", serialCommand);
}

int FindCommand(String serialResponse)
{
    for (uint8_t command = 0; command < arraySize(ColorKineticsCodes); command++)
    {
        if (serialResponse.startsWith(ColorKineticsCodes[command])) return command;
    }
    return ErrorOccurred;
}

int FindCommandByName(String commandName)
{
    for (uint8_t command = 0; command < arraySize(ColorKineticsCodes); command++)
    {
        if (commandName.equals(ColorKineticsCodeNames[command])) return command;
    }
    return ErrorOccurred;
}

int ParseResponseValue(String serialResponse)
{
    if (serialResponse.length() < 5) return 0;
    String hexValue = serialResponse.substring(3); //take last 2 characters of 5 character serialResponse
    return (int)strtol(hexValue, 0, 16); //convert hexadecimal string to integer value
}

void PrintSerialCommand(String serialCommand)
{
    Log.info("Parsed serial command: %s with value: %d", ColorKineticsCodeNames[FindCommand(serialCommand)], ParseResponseValue(serialCommand));
}

void SendSerialCommand(int command, int value)
{
    const char * serialCommand = EncodeSerialCommand(command, value).c_str();
    Log.info("Sending command to serial device: %s", serialCommand);
    Publish(EventReport, String::format("Sending ColorKinetics command: %s with value = %d (%s)", ColorKineticsCodeNames[command], value, serialCommand));
    Serial1.write(serialCommand);
    Serial1.flush(); //ensure serialCommand has been sent to ColorKinetics device
    ReceiveSerialResponse();
}

void ReceiveSerialResponse()
{
    String response = Serial1.readString();
    Log.info(String::format("Raw Response: %s", response.c_str()));
    const char * responseAction = ColorKineticsCodeNames[FindCommand(response)];
    const int responseValue = ParseResponseValue(response);
    Log.info(String::format("ColorKinetics Response: %s with value = %d (%s)", responseAction, responseValue, response.c_str()));
    Publish(EventReport, String::format("ColorKinetics Response: %s with value = %d (%s)", responseAction, responseValue, response.c_str()));
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
    }
    else
    {
        lightingShow = 0;
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
        Log.info("Testing lighting show %d.", lightingShow);
        timeAtLastTestCycle = Time.now();
        SendSerialCommand(SetShow, lightingShow);
    }
}

//callback for event: minneapolis-505FourthAveS-lighting-command
//Commands should be in the form: CommandName:Value
//CommandName should be one of the string values in the 
//e.g. SetShow:5 or TurnLightsOff:0
void TriggerLightingCommand(const char *event, const char *data)
{
    String encodedCommand = String(data);
    String commandName = encodedCommand.substring(0, encodedCommand.indexOf(":"));
    int command = FindCommandByName(commandName);
    if (command > SetShow) return;
    int value = encodedCommand.substring(encodedCommand.indexOf(":") + 1).toInt();
    SendSerialCommand(command, value);
}

//callback for event: minneapolis-505FourthAveS-discharge-measurement-trigger
void TriggerRequest(const char *event, const char *data)
{
    state = DeviceState::Waiting;
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
        Publish(EventReport, String::format("Retrieved discharge measurement of: %d", dischargeMeasurement));
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
        Publish(EventReport, "Measurement request timed out.");
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
        SendSerialCommand(SetShow, lightingShow);
    }
}