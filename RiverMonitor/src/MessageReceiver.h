#include <JsonParserGeneratorRK.h>
#include "Constants.h"

class MessageReceiver
{
public:
	MessageReceiver();
    MessageReceivedResult ReceivePacket(char * event, char * data);
    int ParseMeasurement();

private:
    JsonParser parser;
    time_t timeAtRequest = 0;
};