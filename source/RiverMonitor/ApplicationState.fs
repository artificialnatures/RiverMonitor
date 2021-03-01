module RiverMonitor.ApplicationState

type ExecutionEnvironment =
    | CommandLine
    | OnDevice

type ExecutionStrategy =
    | RetrieveFromUSGS
    | GenerateTestSamples

type Condition =
    | Normal
    | Troubled
    | Failed

let troubledThreshold = System.TimeSpan.FromHours 1.0
let failedThreshold = System.TimeSpan.FromDays 1.0
let troubledIntervalScale = 1.2

type State =
    {
        Environment: ExecutionEnvironment
        Strategy: ExecutionStrategy
        Condition: Condition
        Reading: Result<USGSReading, string>
        PollInterval: System.TimeSpan
        PreviousRetrievalAt: System.DateTime
        RetrieveReading: State -> State
        LogReading: Result<USGSReading, string> -> unit
        LogMessage: string -> unit
        DisplayCondition: Condition -> unit
    }
