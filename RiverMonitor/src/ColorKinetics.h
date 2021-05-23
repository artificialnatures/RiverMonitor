#include "Particle.h"
#include <map>

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

struct ColorKineticsResult
{
    ColorKineticsResponse Response;
    int Value;
};

class ColorKinetics
{
public:
    ColorKinetics();
    ColorKineticsResult SendRequest(ColorKineticsRequest request, int value);
private:
    String SendCode(String code);
};