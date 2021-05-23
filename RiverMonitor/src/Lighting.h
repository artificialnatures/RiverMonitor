#include "Particle.h"

enum class LightingResult
{
    Unchanged,
    Updated,
    Error
};

struct LightingState
{
    LightingResult Result;
    String Message;
    int Show;
};

class Lighting
{
public:
    Lighting();
    LightingState Update(int dischargeMeasurement);
    LightingState GetState();
private:
    int FindLightingShow(int dischargeMeasurement);
    int Show;
    LightingState State;
};