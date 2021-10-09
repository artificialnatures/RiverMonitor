namespace RiverMonitor

open System.IO.Ports

module SerialDevice =
    let listPorts () = SerialPort.GetPortNames()
    let create portName =
        let serialPort = new SerialPort()
        serialPort.PortName <- portName
        serialPort.BaudRate <- 9600
        serialPort.Parity <- Parity.None
        serialPort.DataBits <- 8
        serialPort.StopBits <- StopBits.One
        serialPort.ReadTimeout <- 500
        serialPort.WriteTimeout <- 500
        serialPort.Open()
        serialPort

type SerialDevice(portName) =
    let serialPort = SerialDevice.create portName
    interface Device with
        member this.Mode = DeviceMode.Live
        member this.IsConnected = serialPort.IsOpen
        member this.Connect () = ()
        member this.DisplayCondition condition = ()
        member this.SendRequest request =
            try
                ColorKinetics.buildRequestCode request
                |> string
                |> serialPort.Write
                System.Threading.Thread.Sleep 100
                serialPort.ReadExisting()
                |> Seq.toList
                |> ColorKinetics.parseResponse
                |> Ok
            with
            | _ -> Error "Failed to communicate via serial port."
