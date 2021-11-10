type USGSWaterServicesResponse = FSharp.Data.JsonProvider<"../json/USGSWaterServices_Response_20211109_1.1.json">

module Program =
    open FSharp.Data
    open FSharp.Data.JsonExtensions
    let extractValue (jsonRecord : JsonValue) =
        match Array.tryFind (fun (propertyName, _) -> propertyName = "value") jsonRecord.Properties with
        | Some (_, propertyValue) ->
            match propertyValue with
            | JsonValue.String number -> (float) number
            | JsonValue.Number number -> (float) number
            | JsonValue.Float number -> number
            | _ -> 0.0
        | None -> 0.0
    let extractMeasurement (timeSeries : USGSWaterServicesResponse.TimeSery) =
        let variableCode = Array.head timeSeries.Variable.VariableCode
        let measurement = 
            (Array.head timeSeries.Values).Value
            |> Array.map (fun jd -> extractValue jd.JsonValue)
            |> Array.tryFindBack (fun v -> v > 0.0)
        match measurement with
        | None -> (variableCode.VariableId, "No data")
        | Some number -> (variableCode.VariableId, $"{number}")
    let display o = 
        System.Console.WriteLine("----------------------------------------------")
        System.Console.WriteLine($"{o}")
    let uri = "https://waterservices.usgs.gov/nwis/iv/?sites=05331000&period=P1D&format=json,1.1"
    let reading = USGSWaterServicesResponse.Load(uri)
    let timeSeries = reading.Value.TimeSeries
    let measurements = Array.map extractMeasurement timeSeries
    Array.iter display measurements
