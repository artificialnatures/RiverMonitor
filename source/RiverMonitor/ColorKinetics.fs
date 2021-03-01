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
        | ModeWasSet of int
        | LightsAreOff
        | IntensityWasSet of int
        | NothingWasSet of int
        | ShowWasSet of int
        | ErrorOccurred of int
    let buildRequestCode request value =
        let baseCode =
            match request with
            | TurnLightsOff -> ['X'; '0'; '1'; '0'; '0']
            | SetIntensity -> ['X'; '0'; '2']
            | SetRelativeIntensity -> ['X'; '0'; '3']
            | SetShow -> ['X'; '0'; '4']
        match value with
        | Some value ->
            Hexadecimal.toCharacters value
            |> List.append baseCode
        | None -> baseCode
    let parseResponse characters =
        let integerValue =
            match characters with
            | [_; _; _; sixteens; units] -> Hexadecimal.fromCharacters [sixteens; units]
            | _ -> 0
        match characters with
        | ['Y'; '0'; '0'; _; _] -> Response.ModeWasSet integerValue
        | ['Y'; '0'; '1'; '0'; '0'] -> Response.LightsAreOff
        | ['Y'; '0'; '2'; _; _] -> Response.IntensityWasSet integerValue
        | ['Y'; '0'; '3'; _; _] -> Response.NothingWasSet integerValue
        | ['Y'; '0'; '4'; _; _] -> Response.ShowWasSet integerValue
        | ['Y'; '0'; 'F'; _; _] -> Response.ErrorOccurred integerValue
        | _ -> Response.ErrorOccurred integerValue