namespace RiverMonitor

open System

type Settings =
    {
        ExecutionStrategy : ExecutionStrategy
        USGSUri : string
        SerialPort : string
        MinutesBetweenRetrievals : float
        OpeningTime : TimeSpan
        ClosingTime : TimeSpan
    }

module Settings =
    open Newtonsoft.Json
    let defaultSettings =
        {
            ExecutionStrategy = ExecutionStrategy.GenerateTestSamples
            USGSUri = "https://waterservices.usgs.gov/nwis/iv/?sites=05331000&period=P1D&format=json"
            SerialPort = "/dev/ttyUSB0"
            MinutesBetweenRetrievals = 51.0
            OpeningTime = TimeSpan.Parse("06:00")
            ClosingTime = TimeSpan.Parse("18:00")
        }
    let load () =
        let settingsPath = System.AppContext.BaseDirectory + "settings.json"
        let settings =
            if System.IO.File.Exists(settingsPath) then
                System.Console.WriteLine "Reading Settings:"
                System.IO.File.ReadAllText(settingsPath)
                |> JsonConvert.DeserializeObject<Settings>
            else
                System.Console.WriteLine "Using Default Settings:"
                let json = JsonConvert.SerializeObject defaultSettings
                System.IO.File.WriteAllText(settingsPath, json)
                defaultSettings
        System.Console.WriteLine $"{settings}"
        settings
