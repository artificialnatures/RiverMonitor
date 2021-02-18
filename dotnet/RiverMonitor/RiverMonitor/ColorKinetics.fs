namespace RiverMonitor

module ColorKinetics =
    let serialBaudRate = 9600
    let serialDataBits = 8
    let serialParity = false
    let serialStopBits = 1
    let serialFlowControl = false
    type Command =
        | TurnLightsOff //X0100
        | SetIntensity //X02dd
        | SetRelativeIntensity //X03dd
        | SetShow //X04dd
    type Response =
        | ModeWasSet //Y00dd
        | LightsAreOff //Y0100
        | IntensityWasSet //Y02dd
        | NothingWasSet //Y03dd
        | ShowWasSet //Y04dd
        | ErrorOccurred //Y0Fdd
    let this = "that"
