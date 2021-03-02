namespace RiverMonitor.App

module MeadowF7Device =
    open System.Collections.Generic
    open Meadow
    open Meadow.Devices
    open Meadow.Foundation.Leds
    open Meadow.Gateway.WiFi
    open RiverMonitor
    //Interface with the Meadow F7 microcontroller by Wilderness Labs
    //Refer to: http://developer.wildernesslabs.co/
    
    type MeadowApplication(debugMode) =
        inherit App<F7Micro, MeadowApplication>()
        member this.log : string -> unit = if debugMode then (System.Console.WriteLine) else ignore
        member this.device = MeadowApplication.Device
        member val isConnected = MeadowApplication.Device.WiFiAdapter.IsConnected
        member this.connect() =
            let networkAdapterInitialized = this.device.InitWiFiAdapter().Result
            if networkAdapterInitialized then
                let availableNetworkNames =
                    this.device.WiFiAdapter.Scan() :> IList<WifiNetwork>
                    |> Seq.map (fun discoveredNetwork -> discoveredNetwork.Ssid)
                    |> List.ofSeq
                this.log "Available Networks:"
                List.iter this.log availableNetworkNames
                match Network.findApprovedNetwork availableNetworkNames with
                | Some network ->
                    match this.device.WiFiAdapter.Connect(network.SSID, network.Password).ConnectionStatus with
                    | ConnectionStatus.Success ->
                        this.log $"Connected to {network.SSID}"
                    | _ ->
                        this.log $"Failed to connect to {network.SSID}"
                | None -> ()
        member this.modePin =
            this.device.CreateDigitalInputPort(this.device.Pins.D05)
        member this.onboardLed =
            let redPin = this.device.CreateDigitalOutputPort MeadowApplication.Device.Pins.OnboardLedRed
            let greenPin = this.device.CreateDigitalOutputPort MeadowApplication.Device.Pins.OnboardLedGreen
            let bluePin = this.device.CreateDigitalOutputPort MeadowApplication.Device.Pins.OnboardLedBlue
            RgbLed(redPin, greenPin, bluePin)
        //Meadow Serial Pins
        //COM4 - D00 = RX, D01 = TX
        //COM1 - D13 = RX, D12 = TX
        member this.serialPort : Hardware.ISerialPort =
            let parity =
                match ColorKinetics.serialParity with
                | false -> Hardware.Parity.None
                | _ -> Hardware.Parity.Odd
            let stopBits =
                match ColorKinetics.serialStopBits with
                | 2 -> Hardware.StopBits.Two
                | _ -> Hardware.StopBits.One
            this.device.CreateSerialPort(
                this.device.SerialPortNames.Com1,
                ColorKinetics.serialBaudRate,
                ColorKinetics.serialDataBits,
                parity,
                stopBits,
                256)
        member this.sendRequest request =
            try
                ColorKinetics.buildRequestCode request
                |> Hexadecimal.toBytes
                |> (this.serialPort.Write)
                |> ignore
                System.Threading.Thread.Sleep 100
                let mutable buffer : byte array = Array.zeroCreate 8
                this.serialPort.Read(buffer, 0, 8) |> ignore
                Hexadecimal.fromBytes buffer
                |> ColorKinetics.parseResponse
                |> Ok
            with
            | _ -> Error "Failed to communicate via serial port."
        //TODO: Create Application function to send and receive from serial port...
        
        interface Device with
            member this.Mode = if this.modePin.State then DeviceMode.Live else DeviceMode.Testing
            member this.IsConnected = this.isConnected
            member this.Connect() = this.connect()
            member this.DisplayCondition condition =
                match condition with
                | Normal -> this.onboardLed.SetColor RgbLed.Colors.Green
                | Troubled -> this.onboardLed.SetColor RgbLed.Colors.Yellow
                | Failed -> this.onboardLed.SetColor RgbLed.Colors.Red
    
    let initialize debugMode =
        try
            let app = MeadowApplication(debugMode)
            app.serialPort.Open()
            app.connect()
            app :> Device |> Some
        with
        | _ -> None
