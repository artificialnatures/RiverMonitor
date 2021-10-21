namespace RiverMonitor

open RiverMonitor.ColorKinetics

type ExecutionStrategy =
    | RetrieveFromUSGS
    | GenerateTestSamples

module ExecutionStrategy =
    open System
    let fromString (text : string) =
        if text.ToLower().Contains("test")
        then ExecutionStrategy.GenerateTestSamples
        else ExecutionStrategy.RetrieveFromUSGS
    let toString strategy =
        match strategy with
        | ExecutionStrategy.GenerateTestSamples -> "GenerateTestSamples"
        | ExecutionStrategy.RetrieveFromUSGS -> "RetrieveFromUSGS"

type ApplicationState =
    {
        Strategy: ExecutionStrategy
        USGSUri: string
        Condition: ConnectionCondition
        ErrorMessage: string
        Reading: USGSReading
        PollInterval: System.TimeSpan
        PreviousRetrievalAt: System.DateTime
        PreviousRequest: Request
        Device : Device option
        GenerateReading : unit -> USGSReading
    }

module ApplicationState =
    let troubledThreshold = System.TimeSpan.FromHours 1.0
    let failedThreshold = System.TimeSpan.FromDays 1.0
    let troubledIntervalScale = 1.2
