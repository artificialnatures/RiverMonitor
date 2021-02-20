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
let ``Build hexadecimal request codes`` () =
    let testCases =
        [
            (['X'; '0'; '1'; '0'; '0'],
             ColorKinetics.buildRequestCode ColorKinetics.Request.TurnLightsOff None)
            (['X'; '0'; '2'; 'F'; 'F'],
             ColorKinetics.buildRequestCode ColorKinetics.Request.SetIntensity (Some 255))
            (['X'; '0'; '3'; '2'; '0'],
             ColorKinetics.buildRequestCode ColorKinetics.Request.SetRelativeIntensity (Some 32))
            (['X'; '0'; '4'; '0'; '3'],
             ColorKinetics.buildRequestCode ColorKinetics.Request.SetShow (Some 3))
        ]
    let assertAllEqual (a : char list) (b : char list) =
        List.zip a b
        |> List.iter (Assert.Equal)
    Assert.All(testCases, (fun (expected, actual) -> assertAllEqual expected actual))

[<Fact>]
let ``Convert to hexadecimal characters`` () =
    let testCases =
        [
            (['0'; '0'], Hexadecimal.convert 0)
            (['0'; '1'], Hexadecimal.convert 1)
            (['0'; 'A'], Hexadecimal.convert 10)
            (['6'; '4'], Hexadecimal.convert 100)
            (['7'; '0'], Hexadecimal.convert 112)
            (['F'; 'F'], Hexadecimal.convert 255)
        ]
    let assertAllEqual (a : char list) (b : char list) =
        List.zip a b
        |> List.iter (Assert.Equal)
    Assert.All(testCases, (fun (expected, actual) -> assertAllEqual expected actual))
