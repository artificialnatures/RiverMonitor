module Tests

open System
open Expecto
open RiverMonitor
open ApplicationState

[<Tests>]
let usgsTests =
    test "Parse USGS JSON" {
        let expected = [1..10]
        let json = List.map JsonTestData.buildJson expected
        let actual =
            List.map USGSWaterServices.parseReading json
            |> List.map (fun reading -> reading.IntensityLevel)
        Expect.sequenceEqual actual expected "Reading level should equal expected level"
    }

[<Tests>]
let testPatternTests =
    test "Test Pattern" {
        let levels = [1..10]
        let expected =
            List.replicate 3 levels
            |> List.concat
        let actual =
            Seq.take 30 TestData.samples
            |> List.ofSeq
            |> List.map (fun reading -> reading.IntensityLevel)
        Expect.sequenceEqual actual expected "Levels should be cycle from 1 to 10"
    }

[<Tests>]
let applicationTests =
    [
        test "Poll interval increases when troubled" {
            let initialState = Application.initialState
                                   ExecutionEnvironment.CommandLine
                                   ExecutionStrategy.GenerateTestSamples
                                   None
            let troubledState = {initialState with
                                    Condition = Condition.Troubled
                                    PollInterval = TimeSpan.FromSeconds 1.0}
            let rec generateExpected initialInterval =
                seq {
                    let interval = initialInterval * troubledIntervalScale
                    yield interval
                    yield! generateExpected interval
                }
            let expected =
                generateExpected 1.0
                |> Seq.take 10
                |> List.ofSeq
            let rec generateActual state =
                seq {
                    let adjusted = Application.adjustPollInterval state
                    yield adjusted
                    yield! generateActual adjusted
                }
            let actual =
                generateActual troubledState
                |> Seq.take 10
                |> List.ofSeq
                |> List.map (fun state -> state.PollInterval.TotalSeconds)
            List.zip expected actual
            |> List.map (fun (expectedValue, actualValue) ->
                Expect.floatClose Accuracy.high actualValue expectedValue "Poll interval should increase with each adjustment.")
            |> ignore
        }
        test "Application condition degrades with time" {
            let initialState = Application.initialState
                                   ExecutionEnvironment.CommandLine
                                   ExecutionStrategy.GenerateTestSamples
                                   None
            let normalState = {initialState with
                                    Condition = Condition.Normal
                                    PollInterval = TimeSpan.FromSeconds 1.0}
            let expected =
                [
                    Condition.Normal
                    Condition.Normal
                    Condition.Troubled
                    Condition.Troubled
                    Condition.Failed
                    Condition.Failed
                ]
            let actual =
                let now = System.DateTime.Now
                [
                    troubledThreshold * 0.25
                    troubledThreshold * 0.75
                    troubledThreshold * 1.25
                    failedThreshold * 0.75
                    failedThreshold * 1.25
                    failedThreshold * 2.0
                ]
                |> List.map (fun timeSince -> now.Subtract timeSince)
                |> List.map (fun time -> {normalState with PreviousRetrievalAt = time})
                |> List.map Application.assessCondition
                |> List.map (fun state -> state.Condition)
            Expect.sequenceEqual actual expected "Condition should degrade over time."
        }
    ] |> testList "Application Tests"

[<Tests>]
let hexadecimalTests =
    [
        test "Build hexadecimal request codes" {
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
            let expected = List.map fst testCases
            let actual = List.map snd testCases
            Expect.sequenceEqual actual expected "Code should contain all expected characters."
        }
        test "Parse response codes" {
            let testCases =
                [
                    (ColorKinetics.Response.ModeWasSet 1, ColorKinetics.parseResponse ['Y'; '0'; '0'; '0'; '1'])
                    (ColorKinetics.Response.LightsAreOff, ColorKinetics.parseResponse ['Y'; '0'; '1'; '0'; '0'])
                    (ColorKinetics.Response.IntensityWasSet 160, ColorKinetics.parseResponse ['Y'; '0'; '2'; 'A'; '0'])
                    (ColorKinetics.Response.NothingWasSet 0, ColorKinetics.parseResponse ['Y'; '0'; '3'; '0'; '0'])
                    (ColorKinetics.Response.ShowWasSet 5, ColorKinetics.parseResponse ['Y'; '0'; '4'; '0'; '5'])
                    (ColorKinetics.Response.ErrorOccurred 3, ColorKinetics.parseResponse ['Y'; '0'; 'F'; '0'; '3'])
                ]
            let expected = List.map fst testCases
            let actual = List.map snd testCases
            Expect.sequenceEqual actual expected "Response codes must match."
        }
        test "Convert to hexadecimal characters" {
            let expected =
                [
                    ['0'; '0']
                    ['0'; '1']
                    ['0'; 'A']
                    ['6'; '4']
                    ['7'; '0']
                    ['F'; 'F']
                ]
            let actual =
                [
                    Hexadecimal.toCharacters 0
                    Hexadecimal.toCharacters 1
                    Hexadecimal.toCharacters 10
                    Hexadecimal.toCharacters 100
                    Hexadecimal.toCharacters 112
                    Hexadecimal.toCharacters 255
                ]
            Expect.sequenceEqual actual expected "Expected and actual should be exactly the same character."
        }
        test "Convert from hexadecimal characters" {
            let expected =
                [
                    0
                    1
                    10
                    100
                    112
                    255
                ]
            let actual =
                [
                    Hexadecimal.fromCharacters ['0'; '0']
                    Hexadecimal.fromCharacters ['0'; '1']
                    Hexadecimal.fromCharacters ['0'; 'A']
                    Hexadecimal.fromCharacters ['6'; '4']
                    Hexadecimal.fromCharacters ['7'; '0']
                    Hexadecimal.fromCharacters ['F'; 'F']
                ]
            Expect.sequenceEqual actual expected "Integer values should be equal."
        }
    ] |> testList "Hexadecimal Tests"
