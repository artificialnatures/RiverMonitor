#include "Constants.h"
#include "MessageReceiver.h"
#include "Lighting.h"
#include "ColorKinetics.h"

DeviceState state = DeviceState::Started;
time_t timeAtLastSuccessfulRetrieval = 0;
MessageReceiver * messageReceiver = NULL;
Lighting * lighting = new Lighting();
ColorKinetics * colorKinetics = new ColorKinetics();

void toggleTesting(char *event, char *data)
{
    //write a tester...
    state = DeviceState::Testing;
}

void triggerRequest(char *event, char *data)
{
    RequestMeasurement();
}

void receiveMessagePacket(char *event, char *data)
{
    if (messageReceiver != NULL)
    {
        MessageReceivedResult result = messageReceiver->ReceivePacket(event, data);
        switch (result)
        {
            case MessageReceivedResult::Incomplete:
                state = DeviceState::ReceivingMeasurement;
                break;
            case MessageReceivedResult::Complete:
                state = DeviceState::MeasurementReceived;
                break;
            case MessageReceivedResult::Failed:
                Particle.publish("minneapolis-505FourthAveS-discharge-measurement-retrieved", "Failed to retrieve discharge measurement.", PRIVATE);
                state = DeviceState::Waiting;
                break;
        }
    }
}

void RequestMeasurement()
{
    if (state == DeviceState::Waiting)
    {
        messageReceiver = new MessageReceiver();
        Particle.publish("hook-response/mississippi-stpaul-discharge", "", PRIVATE);
        state = DeviceState::MeasurementRequested;
    }
}

void setup() 
{
    Particle.subscribe("minneapolis-505FourthAveS-test", toggleTesting, MY_DEVICES);
    Particle.subscribe("minneapolis-505FourthAveS-discharge-measurement-trigger", triggerRequest, MY_DEVICES);
    Particle.subscribe("hook-response/mississippi-stpaul-discharge", receiveMessagePacket, MY_DEVICES);
    //Particle.publish("minneapolis-505FourthAveS-discharge-measurement-retrieved", String::format("Retrieved discharge measurement of: %s", "0"));
    //Particle.publish("minneapolis-505FourthAveS-lighting-show", String::format("Setting lighting show to: %d", 1));
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
        break;
    case DeviceState::ReceivingMeasurement:
        break;
    case DeviceState::MeasurementReceived:
        int measurement = messageReceiver->ParseMeasurement();
        Particle.publish("minneapolis-505FourthAveS-discharge-measurement-retrieved", String::format("Retrieved discharge measurement of: %d", measurement), PRIVATE);
        LightingState lightingState = lighting->Update(measurement);
        if (lightingState.Result == LightingResult::Updated)
        {
            state = DeviceState::SettingLightingShow;
        }
        else
        {
            state = DeviceState::Waiting;
        }
        break;
    case DeviceState::SettingLightingShow:
        LightingState lightingState = lighting->GetState();
        ColorKineticsResult result = colorKinetics->SendRequest(ColorKineticsRequest::SetShow, lightingState.Show);
        Particle.publish("minneapolis-505FourthAveS-lighting-show", String::format("Setting lighting show to: %d", lightingState.Show));
        state = DeviceState::Waiting;
        break;
    case DeviceState::Testing:
        //update test cycle...
        break;
    }
    delay(1000); //Loop every second
}