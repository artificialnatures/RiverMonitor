module RiverMonitor.Application

open System
open ApplicationState

let defaultPollInterval strategy =
    match strategy with
    | RetrieveFromUSGS -> 15.0 * 60.0 |> TimeSpan.FromSeconds
    | GenerateTestSamples -> 30.0 |> TimeSpan.FromSeconds

let createLogger state : string -> unit =
    match state.Environment with
    | CommandLine -> Console.WriteLine
    | OnDevice ->
        match state.Device with
        | Some device ->
            match device.Mode with
            | DeviceMode.Live -> ignore
            | DeviceMode.Testing -> Console.WriteLine
        | None -> ignore

let createReadingLogger state : Result<USGSReading, string> -> unit =
    let log = createLogger state
    let logReading result =
        match result with
        | Ok reading -> log (USGSWaterServices.toString reading)
        | Error message -> log message
    logReading

let buildReadingGenerator () =
    let mutable index = -1
    let retrieve () =
        index <- index + 1
        Seq.item index TestData.samples
    retrieve

let conditionColor condition =
    match condition with
    | Normal -> ConsoleColor.Green
    | Troubled -> ConsoleColor.Yellow
    | Failed -> ConsoleColor.Red

let createDisplayCondition (device : Device option) =
    match device with
    | Some device -> device.DisplayCondition
    | None -> (fun condition -> Console.ForegroundColor = conditionColor condition |> ignore)

let initialState environment strategy device =
    {
        Environment = environment
        Strategy = strategy
        Condition = ConnectionCondition.Troubled
        Reading = Seq.head TestData.samples
        PollInterval = defaultPollInterval strategy
        PreviousRetrievalAt = DateTime.MinValue
        PreviousRequest = ColorKinetics.Request.TurnLightsOff
        Device = device
        GenerateReading = buildReadingGenerator()
    }

let chooseMode state =
    let sampledMode =
        match state.Environment with
        | ExecutionEnvironment.OnDevice ->
            match state.Device with
            | Some device ->
                match device.Mode with
                | DeviceMode.Live -> ExecutionStrategy.RetrieveFromUSGS
                | DeviceMode.Testing -> ExecutionStrategy.GenerateTestSamples
            | None -> state.Strategy
        | ExecutionEnvironment.CommandLine -> state.Strategy
    if state.Strategy = sampledMode
    then state
    else {state with Strategy = sampledMode}

let verifyConnection state =
    match (state.Environment, state.Strategy) with
    | ExecutionEnvironment.OnDevice, ExecutionStrategy.RetrieveFromUSGS ->
        match state.Device with
        | Some device ->
            if not device.IsConnected then device.Connect()
        | None -> ()
    | _, _ -> ()
    state

let retrieveReading state =
    let retrieveFromUSGS state =
        let response = USGSWaterServices.retrieveLatest()
        response |> createReadingLogger state
        match response with
        | Ok reading -> {state with Reading = reading; PreviousRetrievalAt = DateTime.Now}
        | Error _ -> {state with Reading = state.GenerateReading()}
    let nextState =
        match (state.Environment, state.Strategy, state.Device) with
        | ExecutionEnvironment.OnDevice, ExecutionStrategy.RetrieveFromUSGS, Some device ->
            match device.IsConnected with
            | true -> retrieveFromUSGS state
            | false -> {state with Reading = state.GenerateReading()}
        | ExecutionEnvironment.CommandLine, ExecutionStrategy.RetrieveFromUSGS, None ->
            retrieveFromUSGS state
        | _, _, _ ->
            {state with Reading = state.GenerateReading(); PreviousRetrievalAt = DateTime.Now}
    USGSWaterServices.toString nextState.Reading
    |> createLogger state
    nextState

let adjustPollInterval state =
    let pollInterval =
        match state.Condition with
        | Normal -> state.PollInterval
        | Troubled -> state.PollInterval.TotalSeconds * troubledIntervalScale |> TimeSpan.FromSeconds
        | Failed -> TimeSpan.FromDays 1.0
    {state with PollInterval = pollInterval}

let assessCondition state =
    let timeSincePreviousRetrieval = DateTime.Now - state.PreviousRetrievalAt
    let condition =
        if timeSincePreviousRetrieval > failedThreshold
        then Failed
        else if timeSincePreviousRetrieval > troubledThreshold
        then Troubled
        else Normal
    {state with Condition = condition}

let displayCondition state =
    $"Condition: {ConnectionCondition.toString state.Condition}"
    |> createLogger state
    match state.Environment with
    | ExecutionEnvironment.OnDevice ->
        match state.Device with
        | Some device -> device.DisplayCondition state.Condition
        | None -> ()
    | ExecutionEnvironment.CommandLine -> ()
    state

let updateLights state =
    let log = createLogger state
    let request = ColorKinetics.Request.SetShow state.Reading.IntensityLevel
    if request = state.PreviousRequest then
        log $"No request sent to Color Kinetics, already {ColorKinetics.requestToString request}"
        state
    else
        match state.Environment with
        | ExecutionEnvironment.CommandLine ->
            log $"Would send request to Color Kinetics: {ColorKinetics.requestToString request}"
        | ExecutionEnvironment.OnDevice ->
            log $"Sending request to Color Kinetics: {ColorKinetics.requestToString request}"
            match state.Device with
            | Some device ->
                match device.SendRequest request with
                | Ok response ->
                    log $"Received response from Color Kinetics: {ColorKinetics.responseToString response}"
                | Error message ->
                    log $"Error from Color Kinetics: {message}"
            | None -> log $"Error: Device not initialized"
        {state with PreviousRequest = request}
