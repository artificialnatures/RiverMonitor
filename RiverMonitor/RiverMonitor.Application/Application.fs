module RiverMonitor.Application

open System
open ApplicationState

let defaultPollInterval strategy =
    match strategy with
    | RetrieveFromUSGS -> 15.0 |> TimeSpan.FromMinutes
    | GenerateTestSamples -> 30.0 |> TimeSpan.FromSeconds

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

let initialState strategy uri device =
    {
        Strategy = strategy
        USGSUri = uri
        Condition = ConnectionCondition.Troubled
        ErrorMessage = ""
        Reading = Seq.head TestData.samples
        PollInterval = defaultPollInterval strategy
        PreviousRetrievalAt = DateTime.MinValue
        PreviousRequest = ColorKinetics.Request.TurnLightsOff
        Device = device
        GenerateReading = buildReadingGenerator()
    }

let retrieveReading state =
    let retrieveFromUSGS state =
        let response = USGSWaterServices.retrieveLatest()
        match response with
        | Ok reading -> {state with Reading = reading; PreviousRetrievalAt = DateTime.Now; ErrorMessage = ""}
        | Error message -> {state with ErrorMessage = message}
    match state.Strategy with
    | ExecutionStrategy.RetrieveFromUSGS ->
        retrieveFromUSGS state
    | ExecutionStrategy.GenerateTestSamples ->
        {state with Reading = state.GenerateReading(); PreviousRetrievalAt = DateTime.Now; ErrorMessage = ""}

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

let display state =
    Console.WriteLine $"{state}"
    state

let updateLights state =
    let request = ColorKinetics.Request.SetShow state.Reading.IntensityLevel
    if request = state.PreviousRequest then
        Console.WriteLine $"No request sent to Color Kinetics, already {ColorKinetics.requestToString request}"
        state
    else
        match state.Device with
        | Some device ->
            Console.WriteLine $"Sending request to Color Kinetics: {ColorKinetics.requestToString request}"
            match device.SendRequest request with
            | Ok response ->
                Console.WriteLine $"Received response from Color Kinetics: {ColorKinetics.responseToString response}"
            | Error message ->
                Console.WriteLine $"Error from Color Kinetics: {message}"
        | None ->
            Console.WriteLine $"No device initialized. Would send request to Color Kinetics: {ColorKinetics.requestToString request}"
        {state with PreviousRequest = request}
