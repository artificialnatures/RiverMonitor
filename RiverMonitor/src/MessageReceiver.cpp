#include "MessageReceiver.h"
#include "Particle.h"

MessageReceiver::MessageReceiver() 
{
    this->timeAtRequest = Time.now();
}

MessageReceivedResult MessageReceiver::ReceivePacket(char * event, char * data)
{
    parser.addChunkedData(event, data);
    if (parser.parse())
    {
        return MessageReceivedResult::Complete;
    }
    if (Time.now() - this->timeAtRequest > SecondsBeforeRequestFails)
    {
        return MessageReceivedResult::Failed;
    }
    return MessageReceivedResult::Incomplete;
    /*
    String messageIndexText = strrchr(event, '/');
    int messageIndex = messageIndexText.toInt();
    packets[messageIndex] = new String(data);
    for (int index = 0; index < sizeof(packets); index++)
    {
        if (packets[index] == NULL)
        {
            return MessageReceivedResult::Incomplete;
        }
        if (packets[index]->length() < 512 && packets[index]->charAt(sizeof(packets) - 1) == '}')
        {
            //multi-part messages will be 512 characters long
            //execpt for the final message, which will be fewer
            return MessageReceivedResult::Complete;
        }
    }
    return MessageReceivedResult::Failed;
    */
}

int MessageReceiver::ParseMeasurement()
{
    /* Parse the JSON response from USGS Water Services
     * See an example: https://waterservices.usgs.gov/nwis/iv/?format=json&sites=05331000&parameterCd=00060&siteStatus=all
     */
    String measurementText = this->parser.getReference().key("value").key("timeSeries").index(0).key("values").index(0).key("value").index(0).key("value").valueString();
    return measurementText.toInt();
    /*
    JsonParser parser;
    for (int index = 0; index < sizeof(packets); index++)
    {
        if (packets[index] != NULL)
        {
            
        }
    }
    */
}