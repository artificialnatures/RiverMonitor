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

let createRetrieve strategy =
    match strategy with
    | RetrieveFromUSGS ->
        let retrieve state =
            let reading = USGSWaterServices.retrieveLatest ()
            {state with Reading = reading; PreviousRetrievalAt = DateTime.Now}
        retrieve
    | GenerateTestSamples ->
        let mutable index = -1
        let retrieve state =
            index <- index + 1
            let reading = Seq.item index TestData.samples
            {state with Reading = Ok reading; PreviousRetrievalAt = DateTime.Now}
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
        Condition = Condition.Troubled
        Reading = Seq.head TestData.samples |> Ok
        PollInterval = defaultPollInterval strategy
        PreviousRetrievalAt = DateTime.MinValue
        RetrieveReading = createRetrieve strategy
        LogReading = createReadingLogger environment
        LogMessage = createLogger environment
        DisplayCondition = createDisplayCondition device
    }

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
    state.DisplayCondition state.Condition
    state

let logReading state =
    state.LogReading state.Reading
    state
