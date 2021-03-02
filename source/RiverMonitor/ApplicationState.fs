module RiverMonitor.ApplicationState

type ExecutionEnvironment =
    | CommandLine
    | OnDevice

type ExecutionStrategy =
    | RetrieveFromUSGS
    | GenerateTestSamples

let troubledThreshold = System.TimeSpan.FromHours 1.0
let failedThreshold = System.TimeSpan.FromDays 1.0
let troubledIntervalScale = 1.2

type State =
    {
        Environment: ExecutionEnvironment
        Strategy: ExecutionStrategy
        Condition: ConnectionCondition
        Reading: USGSReading
        PollInterval: System.TimeSpan
        PreviousRetrievalAt: System.DateTime
        Device : Device option
        GenerateReading : unit -> USGSReading
    }
