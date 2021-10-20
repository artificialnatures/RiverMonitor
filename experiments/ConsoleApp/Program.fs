open System
open System.Threading
open RiverMonitor
open Hours

[<EntryPoint>]
let main _ =
    Console.WriteLine "Available Serial Ports:"
    for port in SerialDevice.listPorts () do Console.WriteLine $"  - {port}"
    Console.WriteLine "Specify settings in settings.json"
    let settings = Settings.load ()
    let device = SerialDevice.create settings.SerialPort
    let evaluate state =
        let now = DateTime.Now.TimeOfDay
        if now.isWithin settings.OpeningTime settings.ClosingTime then
            Application.retrieveReading state
            |> Application.updateLights
            |> Application.assessCondition
            |> Application.adjustPollInterval
            |> Application.display
        else
            Console.WriteLine $"It's {now} - we're closed. We'll be open again tomorrow from {settings.OpeningTime} to {settings.ClosingTime}."
            state
    let wait state =
        Thread.Sleep state.PollInterval
        state
    let rec program state =
        evaluate state
        |> wait
        |> program
    let initialState = Application.initialState
                           settings.ExecutionStrategy
                           settings.USGSUri
                           device
    program {initialState with PollInterval = TimeSpan.FromMinutes settings.MinutesBetweenRetrievals} |> ignore
    0
