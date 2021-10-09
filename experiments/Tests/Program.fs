open System
open RiverMonitor

module Program =
    open Expecto
    
    [<EntryPoint>]
    //Serial port test:
    let  main args =
        Console.WriteLine "Serial Ports:"
        for port in SerialDevice.listPorts () do Console.WriteLine $"  - {port}"
        let device = SerialDevice("/dev/tty.URT2") :> Device
        let request = ColorKinetics.Request.SetShow 3
        Console.WriteLine "\nResult:"
        match device.SendRequest request with
        | Ok message ->
            match message with
            | ColorKinetics.Response.ShowWasSet showNumber ->
                Console.WriteLine $"Success! Set show to {showNumber}"
            | _ -> Console.WriteLine "Error"
        | _ -> Console.WriteLine "Error"
        -1
    //Run all tests:
    //let  main args =
    //    runTestsInAssemblyWithCLIArgs [] args
