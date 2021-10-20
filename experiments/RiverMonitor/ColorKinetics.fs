namespace RiverMonitor

module ColorKinetics =
    let serialBaudRate = 9600
    let serialDataBits = 8
    let serialParity = false
    let serialStopBits = 1
    let serialFlowControl = false
    type Request =
        | TurnLightsOff
        | SetIntensity of int
        | SetRelativeIntensity of int
        | SetShow of int
    let requestToString request =
        match request with
        | TurnLightsOff -> "Turn lights off"
        | SetIntensity value -> $"Set intensity: {value}"
        | SetRelativeIntensity value -> $"Set relative intensity: {value}"
        | SetShow value -> $"Set show: {value}"
    type Response =
        | ModeWasSet of int
        | LightsAreOff
        | IntensityWasSet of int
        | NothingWasSet of int
        | ShowWasSet of int
        | ErrorOccurred of int
    let responseToString response =
        match response with
        | ModeWasSet value -> $"Mode was set to {value}"
        | LightsAreOff -> "Lights were turned off"
        | IntensityWasSet value -> $"Intensity was set to {value}"
        | NothingWasSet value -> $"Nothing was set to {value}"
        | ShowWasSet value -> $"Show was set to {value}"
        | ErrorOccurred value -> $"Error {value} occurred"
    let buildRequestCode request =
        match request with
        | TurnLightsOff ->
            ['X'; '0'; '1'; '0'; '0']
        | SetIntensity value ->
            Hexadecimal.toCharacters value
            |> List.append ['X'; '0'; '2']
        | SetRelativeIntensity value ->
            Hexadecimal.toCharacters value 
            |> List.append ['X'; '0'; '3']
        | SetShow value ->
            Hexadecimal.toCharacters value
            |> List.append ['X'; '0'; '4']
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
