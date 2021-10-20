#r "nuget:System.IO.Ports"

open System.IO.Ports

let connect portName =
    let serialPort = new SerialPort()
    serialPort.PortName <- portName
    serialPort.BaudRate <- 9600
    serialPort.Parity <- Parity.None
    serialPort.DataBits <- 8
    serialPort.StopBits <- StopBits.One
    serialPort.ReadTimeout <- 500
    serialPort.WriteTimeout <- 500
    serialPort

let port = connect "/dev/tty.usbserial-14510"
port.Open()
let message = ['X'; '0'; '4'; '0'; '8']
let messageString = 
    List.map (string) message
    |> List.fold (+) ""
printfn "Sending: %s" messageString
port.Write(messageString) //both string and character at a time work
//for character in message do port.Write(character |> string)
System.Threading.Thread.Sleep 100
let response = port.ReadExisting()
printfn "Response: %s" response
port.Close()