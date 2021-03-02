open System.Threading
open RiverMonitor
open RiverMonitor.App
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
    let debugMode = true
    let device = MeadowF7Device.initialize debugMode
    let initialState = Application.initialState
                           ExecutionEnvironment.OnDevice
                           ExecutionStrategy.RetrieveFromUSGS
                           device
    program initialState |> ignore
    0
