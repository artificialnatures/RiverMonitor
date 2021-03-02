module RiverMonitor.Application

open System
open ApplicationState

let defaultPollInterval strategy =
    match strategy with
    | RetrieveFromUSGS -> 15.0 * 60.0 |> TimeSpan.FromSeconds
    | GenerateTestSamples -> 30.0 |> TimeSpan.FromSeconds

let createLogger environment : string -> unit =
    match environment with
    | CommandLine -> Console.WriteLine
    | _ -> ignore

let createReadingLogger environment : Result<USGSReading, string> -> unit =
    match environment with
    | CommandLine ->
        let log (reading : Result<USGSReading, string>) =
            match reading with
            | Ok reading -> Console.WriteLine(USGSWaterServices.toString reading)
            | Error message -> Console.WriteLine(message)
        log
    | _ -> ignore

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
        match USGSWaterServices.retrieveLatest() with
        | Ok reading -> {state with Reading = reading; PreviousRetrievalAt = DateTime.Now}
        | Error _ -> {state with Reading = state.GenerateReading()}
    match (state.Environment, state.Strategy, state.Device) with
    | ExecutionEnvironment.OnDevice, ExecutionStrategy.RetrieveFromUSGS, Some device ->
        match device.IsConnected with
        | true -> retrieveFromUSGS state
        | false -> {state with Reading = state.GenerateReading()}
    | ExecutionEnvironment.CommandLine, ExecutionStrategy.RetrieveFromUSGS, None ->
        retrieveFromUSGS state
    | _, _, _ ->
        {state with Reading = state.GenerateReading(); PreviousRetrievalAt = DateTime.Now}

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
    match state.Environment with
    | ExecutionEnvironment.OnDevice ->
        match state.Device with
        | Some device -> device.DisplayCondition state.Condition
        | None -> ()
    | ExecutionEnvironment.CommandLine ->
        Console.WriteLine $"Condition: {nameof state.Condition}"
    state

let logReading state =
    match state.Environment with
    | ExecutionEnvironment.CommandLine ->
        Console.WriteLine(USGSWaterServices.toString state.Reading)
    | _ -> ()
    state
