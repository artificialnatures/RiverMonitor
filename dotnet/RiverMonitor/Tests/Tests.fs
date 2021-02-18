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

[<Fact>]
let ``Convert to hexadecimal characters`` () =
    let testCases =
        [
            (0, ['0'; '0'])
            (1, ['0'; '1'])
            (10, ['0'; 'A'])
            (100, ['6'; '4'])
            (112, ['7'; '0'])
            (255, ['F'; 'F'])
        ]
    let expectedResults = List.map snd testCases
    let results =
        List.map fst testCases
        |> List.map Hexdecimal.convert
        |> List.zip expectedResults
    let assertAllEqual (a : char list) (b : char list) =
        List.zip a b
        |> List.iter (fun (expected, actual) -> Assert.Equal(expected, actual))
    Assert.All(results, (fun (expected, actual) -> assertAllEqual expected actual))
