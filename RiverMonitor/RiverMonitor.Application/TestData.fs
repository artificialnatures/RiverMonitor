module RiverMonitor.TestData

open System

let randomValue lowerBound upperBound =
    (Random().NextDouble() * (upperBound - lowerBound)) + lowerBound

let rec cycle items =
    seq { yield! items; yield! cycle items }

let generate level =
    {
        Site = "Test Site"
        Time = DateTime.Now
        Temperature = randomValue 32.0 80.0
        DischargeVolume = randomValue 1000.0 10000.0
        GageHeight = randomValue 1.0 50.0
        Elevation = randomValue 650.0 700.0
        Velocity = randomValue 0.3 30.0
        IntensityLevel = level
    }

let samples =
    cycle [1..10]
    |> Seq.map generate
