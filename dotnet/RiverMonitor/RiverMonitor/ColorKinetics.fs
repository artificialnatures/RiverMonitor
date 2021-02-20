namespace RiverMonitor

module ColorKinetics =
    let serialBaudRate = 9600
    let serialDataBits = 8
    let serialParity = false
    let serialStopBits = 1
    let serialFlowControl = false
    type Request =
        | TurnLightsOff
        | SetIntensity
        | SetRelativeIntensity
        | SetShow
    type Response =
        | ModeWasSet //Y00dd
        | LightsAreOff //Y0100
        | IntensityWasSet //Y02dd
        | NothingWasSet //Y03dd
        | ShowWasSet //Y04dd
        | ErrorOccurred //Y0Fdd
    let buildRequestCode request value =
        let baseCode =
            match request with
            | TurnLightsOff -> ['X'; '0'; '1'; '0'; '0']
            | SetIntensity -> ['X'; '0'; '2']
            | SetRelativeIntensity -> ['X'; '0'; '3']
            | SetShow -> ['X'; '0'; '4']
        match value with
        | Some value ->
            Hexadecimal.convert value
            |> List.append baseCode
        | None -> baseCode
