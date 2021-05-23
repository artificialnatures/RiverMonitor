#include "Lighting.h"
#include "Constants.h"

Lighting::Lighting()
{
    this->Show = -1;
}

LightingState Lighting::Update(int dischargeMeasurement)
{
    LightingState state;
    int show = FindLightingShow(dischargeMeasurement);
    if (show != this->Show)
    {
        this-> Show = show;
        state.Result = LightingResult::Updated;
        state.Message = String::format("Setting lighting show to %d", this->Show);
    }
    else
    {
        state.Result = LightingResult::Unchanged;
        state.Message = "Lighting show unchanged.";
    }
    state.Show = this->Show;
    this->State = state;
    return this->State;
}

LightingState Lighting::GetState()
{
    return this->State;
}

int Lighting::FindLightingShow(int dischargeMeasurement)
{
    for (int index = 0; index < arraySize(UpperDischargeBounds); index++) 
    {
        if (dischargeMeasurement < UpperDischargeBounds[index])
        {
            return index + 1;
        }
    }
    return 1;
}