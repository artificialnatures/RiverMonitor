const time_t SecondsBetweenRetrievals = 3600;
const time_t SecondsBeforeRequestFails = 30;

enum class DeviceState
{
    Started,
    Waiting,
    MeasurementRequested,
    ReceivingMeasurement,
    MeasurementReceived,
    SettingLightingShow,
    Testing
};

enum class MessageReceivedResult
{
    Failed,
    Incomplete,
    Complete
};

//Discharge values for Mississippi River at St. Paul, MN in cubic feet per second
int UpperDischargeBounds[] = 
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