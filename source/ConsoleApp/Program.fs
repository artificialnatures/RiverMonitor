open System
open System.Threading
open RiverMonitor
open RiverMonitor.ApplicationState

[<EntryPoint>]
let main _ =
    let rec program state =
        let nextState =
            Application.chooseMode state
            |> Application.verifyConnection
            |> Application.retrieveReading
            |> Application.assessCondition
            |> Application.adjustPollInterval
            |> Application.displayCondition
            |> Application.logReading
        Thread.Sleep nextState.PollInterval
        program nextState
    let initialState = Application.initialState
                           ExecutionEnvironment.CommandLine
                           ExecutionStrategy.RetrieveFromUSGS
                           None
    program {initialState with PollInterval = TimeSpan.FromSeconds 10.0} |> ignore
    0
