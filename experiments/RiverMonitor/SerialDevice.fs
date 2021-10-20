namespace RiverMonitor

open System
open System.IO.Ports

type SerialDevice(portName) =
    let serialPort =
        let serialPort = new SerialPort()
        serialPort.PortName <- portName
        serialPort.BaudRate <- 9600
        serialPort.Parity <- Parity.None
        serialPort.DataBits <- 8
        serialPort.StopBits <- StopBits.One
        serialPort.ReadTimeout <- 500
        serialPort.WriteTimeout <- 500
        serialPort
    interface Device with
        member this.SendRequest request =
            try
                serialPort.Open()
                let message = ColorKinetics.buildRequestCode request
                for character in message do serialPort.Write(character |> string)
                System.Threading.Thread.Sleep 100
                let response = serialPort.ReadExisting()
                serialPort.Close()
                response
                |> Seq.toList
                |> ColorKinetics.parseResponse
                |> Ok
                
            with
            | _ -> Error "Failed to communicate via serial port."

module SerialDevice =
    let listPorts () = SerialPort.GetPortNames() |> List.ofArray
    let create portName =
        let ports = listPorts ()
        if List.contains portName ports then
            SerialDevice(portName) :> Device |> Some
        else
            Console.WriteLine $"Error: Serial port {portName} not found."
            None
