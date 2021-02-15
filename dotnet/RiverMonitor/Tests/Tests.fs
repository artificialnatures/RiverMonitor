module Tests

open Xunit
open RiverMonitor

[<Fact>]
let ``Parse USGS json`` () =
    let levels = [1..10]
    let json = List.map TestData.buildJson levels
    let readings = List.map USGSWaterServices.parseReading json
    let results = List.zip levels readings
    Assert.All(results, (fun (expectedLevel, reading) -> Assert.Equal(expectedLevel, reading.IntensityLevel)))
