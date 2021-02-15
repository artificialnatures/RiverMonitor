namespace RiverMonitor

open System
open System.IO
open System.Text
open FSharp.Data

//JsonProvider handles retrieval and parsing of USGS Water Services data.
//The sample .json file tells the JsonProvider the data structure to expect.
//Refer to https://waterservices.usgs.gov/ for details.
type USGSWaterServicesResponse = JsonProvider<"json/USGSWaterServices_Response_20210214.json">

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
        Temperature : float
        DischargeVolume : float
        GageHeight : float
        Elevation : float
        Velocity : float
        IntensityLevel : int
    }

module USGSWaterServices =
    let siteId = "05331000"
    let period = "P1D"
    let format = "json"
    let uri = $"https://waterservices.usgs.gov/nwis/iv/?sites={siteId}&period={period}&format={format}"
    let latestValue (series : USGSWaterServicesResponse.TimeSery) =
        let value =
            (Array.head series.Values).Value
            |> Array.sortBy (fun v -> v.DateTime.DateTime)
            |> Array.head
        (float) value.Value
    let readSiteName (series : USGSWaterServicesResponse.TimeSery array) =
        (Array.head series).SourceInfo.SiteName
    let readLatestTime (series : USGSWaterServicesResponse.TimeSery array) =
        let firstSeries = Array.head series
        let value =
            (Array.head firstSeries.Values).Value
            |> Array.sortBy (fun v -> v.DateTime.DateTime)
            |> Array.head
        value.DateTime.DateTime
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
        let matchingSeries = Array.tryFind (fun s -> extractVariableId s = variableId) series
        match matchingSeries with
        | Some matchingSeries -> latestValue matchingSeries
        | None -> 0.0
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
        {
            Site = readSiteName timeSeries
            Time = readLatestTime timeSeries
            Temperature = findValueForVariable Temperature timeSeries
            DischargeVolume = dischargeVolume
            GageHeight = findValueForVariable GageHeight timeSeries
            Elevation = findValueForVariable Elevation timeSeries
            Velocity = findValueForVariable Velocity timeSeries
            IntensityLevel = findDischargeLevel dischargeVolume
        }
    let retrieveLatest () =
        USGSWaterServicesResponse.Load(uri)
        |> assembleReading
    let parseReading (jsonText : string) =
        use stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonText))
        USGSWaterServicesResponse.Load(stream)
        |> assembleReading
    let toString reading =
        [
            $"Site: {reading.Site} at {reading.Time}"
            "----------------------------------------";
            $"Temperature: {reading.Temperature} degrees fahrenheit"
            $"Discharge Volume: {reading.DischargeVolume} cubic feet per second"
            $"Gage Height: {reading.GageHeight} feet"
            $"Elevation: {reading.Elevation} feet"
            $"Velocity: {reading.Velocity} feet per second"
            $"Overall Scale: {reading.IntensityLevel}"
        ]
        |> String.concat Environment.NewLine
