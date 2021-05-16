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
            |> Application.updateLights
            |> Application.assessCondition
            |> Application.adjustPollInterval
            |> Application.displayCondition
        Thread.Sleep nextState.PollInterval
        program nextState
    let debugMode = true
    let device = MeadowF7Device.initialize debugMode
    let initialState = Application.initialState
                           ExecutionEnvironment.OnDevice
                           ExecutionStrategy.RetrieveFromUSGS
                           device
    program {initialState with PollInterval = System.TimeSpan.FromSeconds 30.0} |> ignore
    0
