open System
open System.Threading
open RiverMonitor
open RiverMonitor.ApplicationState

[<EntryPoint>]
let main _ =
    Console.WriteLine "Serial Ports:"
    for port in SerialDevice.listPorts () do Console.WriteLine $"  - {port}"
    let device = SerialDevice("/dev/tty.URT1") :> Device |> Some
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
    let initialState = Application.initialState
                           ExecutionEnvironment.CommandLine
                           ExecutionStrategy.GenerateTestSamples
                           device
    program {initialState with PollInterval = TimeSpan.FromSeconds 30.0} |> ignore
    0
