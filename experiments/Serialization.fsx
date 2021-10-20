#r "nuget:Newtonsoft.Json"

open Newtonsoft.Json

type ExecutionStrategy =
    | RetrieveFromUSGS
    | GenerateTestSamples

type Settings =
    {
        Strategy : ExecutionStrategy
        SerialPort : string
        MinutesBetweenRetrievals : float
    }

let defaultSettings = 
    {
        Strategy = ExecutionStrategy.GenerateTestSamples
        SerialPort = "/dev/ttyUSB0"
        MinutesBetweenRetrievals = 51.0
    }
let json = JsonConvert.SerializeObject defaultSettings
System.Console.WriteLine json

printfn "%O" defaultSettings

System.Console.WriteLine $"{defaultSettings}"