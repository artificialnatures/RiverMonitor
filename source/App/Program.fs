open System.Threading
open RiverMonitor
open RiverMonitor.ApplicationState

[<EntryPoint>]
let main _ =
    let rec program state =
        let nextState =
            state.RetrieveReading state
            |> Application.assessCondition
            |> Application.adjustPollInterval
            |> Application.displayCondition
            |> Application.logReading
        Thread.Sleep nextState.PollInterval
        program nextState
    let device = MeadowDevice() :> Device |> Some
    let initialState = Application.initialState
                           ExecutionEnvironment.CommandLine
                           ExecutionStrategy.GenerateTestSamples
                           device
    program initialState |> ignore
    0
