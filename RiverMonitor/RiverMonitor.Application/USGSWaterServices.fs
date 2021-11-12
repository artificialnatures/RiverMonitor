namespace RiverMonitor

open System
open System.IO
open System.Text

//JsonProvider handles retrieval and parsing of USGS Water Services data.
//The sample .json file tells the JsonProvider the data structure to expect.
//Refer to https://waterservices.usgs.gov/ for details.
//This program uses the feed at:
//https://waterservices.usgs.gov/nwis/iv/?sites=05331000&period=P1D&format=json,1.1
//For single readings: https://waterservices.usgs.gov/nwis/iv/?format=json,1.1&sites=05331000&parameterCd=00060&siteStatus=all
type USGSWaterServicesResponse = FSharp.Data.JsonProvider<"../json/USGSWaterServices_Response_20211109_1.1.json">

type USGSVariableName =
    | Temperature
    | DischargeVolume
    | GageHeight
    | Elevation
    | Velocity

type USGSReading =
    {
        Site : string
        Time : DateTime
        Temperature : float option
        DischargeVolume : float option
        GageHeight : float option
        Elevation : float option
        Velocity : float option
        IntensityLevel : int option
    }

module USGSWaterServices =
    open FSharp.Data

    let siteId = "05331000"
    let period = "P1D"
    let format = "json,1.1"
    let uri = $"https://waterservices.usgs.gov/nwis/iv/?sites={siteId}&period={period}&format={format}"
    let extractValue (jsonRecord : JsonValue) =
        let isValueProperty (property : string * JsonValue) =
            let propertyName, _ = property
            propertyName = "value"
        match Array.tryFind isValueProperty (jsonRecord.Properties()) with
        | Some (_, propertyValue) ->
            match propertyValue with
            | JsonValue.String number -> (float) number
            | JsonValue.Number number -> (float) number
            | JsonValue.Float number -> number
            | _ -> 0.0
        | None -> 0.0
    let extractMeasurement (timeSeries : USGSWaterServicesResponse.TimeSery) =
        (Array.head timeSeries.Values).Value
        |> Array.map (fun jd -> extractValue jd.JsonValue)
        |> Array.tryFindBack (fun v -> v > 0.0)
    let readSiteName (series : USGSWaterServicesResponse.TimeSery array) =
        (Array.head series).SourceInfo.SiteName
    let extractVariableId (series : USGSWaterServicesResponse.TimeSery) =
        (Array.head series.Variable.VariableCode).VariableId
    let variableNameToId =
        [
            (Temperature, 45807043)
            (DischargeVolume, 45807197)
            (GageHeight, 45807202)
            (Velocity, 52333322)
            (Elevation, 51438657)
        ]
        |> Map.ofList
    let findValueForVariable variableName (series : USGSWaterServicesResponse.TimeSery array) =
        let variableId = Map.find variableName variableNameToId
        match Array.tryFind (fun s -> extractVariableId s = variableId) series with
        | None -> None
        | Some series -> extractMeasurement series
    let upperBounds =
        [
            7000.0
            10000.0
            15000.0
            20000.0
            30000.0
            40000.0
            50000.0
            60000.0
            75000.0
        ]
    let levels = [1..List.length upperBounds + 1]
    let dischargeLevels =
        List.append upperBounds [Double.MaxValue]
        |> List.zip levels 
    let findDischargeLevel dischargeVolume =
        List.find (fun (_, upperBound) -> dischargeVolume < upperBound) dischargeLevels
        |> fst
    let findBoundsForDischargeLevel level =
        List.concat [[0.0]; upperBounds; [100000.0]]
        |> List.pairwise
        |> List.zip levels
        |> List.find (fun (candidateLevel, _) -> candidateLevel = level)
        |> snd
    let assembleReading (response : USGSWaterServicesResponse.Root) =
        let timeSeries = response.Value.TimeSeries
        let dischargeVolume = findValueForVariable DischargeVolume timeSeries
        let intensityLevel =
            match dischargeVolume with
            | None -> None
            | Some value -> findDischargeLevel value |> Some
        {
            Site = readSiteName timeSeries
            Time = DateTime.Now
            Temperature = findValueForVariable Temperature timeSeries
            DischargeVolume = dischargeVolume
            GageHeight = findValueForVariable GageHeight timeSeries
            Elevation = findValueForVariable Elevation timeSeries
            Velocity = findValueForVariable Velocity timeSeries
            IntensityLevel = intensityLevel
        }
    let retrieveLatest () =
        try
            USGSWaterServicesResponse.Load(uri)
            |> assembleReading
            |> Ok
        with
        | _ as error -> Error $"Failed to retrieve USGS data. ({error.GetType().Name}: {error.Message})"
    let parseReading (jsonText : string) =
        use stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonText))
        USGSWaterServicesResponse.Load(stream)
        |> assembleReading
    let measurementDiscription measurementValue label unitsText =
        match measurementValue with
        | Some value -> $"{label}: {value} {unitsText}"
        | None -> "No data available"
    let toString reading =
        [
            $"Site: {reading.Site} at {reading.Time}"
            "----------------------------------------"
            measurementDiscription reading.Temperature "Temperature" "degrees fahrenheit"
            measurementDiscription reading.DischargeVolume "Discharge Volume" "cubic feet per second"
            measurementDiscription reading.GageHeight "Gage Height" "feet"
            measurementDiscription reading.Elevation "Elevation" "feet"
            measurementDiscription reading.Velocity "Velocity" "feet per second"
            $"Overall Scale: {reading.IntensityLevel}"
        ]
        |> String.concat Environment.NewLine
