namespace RiverMonitor

open System
open FSharp.Data

type USGSResponse = JsonProvider<"USGSSampleData_Site05331000.json", EmbeddedResource="RiverMonitor, USGSSampleData_Site05331000.json">

type ScaleCategory =
    | Low
    | Medium
    | High
    | VeryHigh

type USGSVariable =
    | Temperature
    | Discharge
    | GageHeight
    | Velocity

module USGS =
    let mississippiAtStPaulSiteName = "Mississippi River at St. Paul, Minnesota"
    let mississippiAtStPaulUri = "https://waterservices.usgs.gov/nwis/iv/?sites=05331000&period=P1D&format=json"
    let variableIdMap =
        [
            (45807043, Temperature);
            (45807197, Discharge);
            (45807202, GageHeight);
            (52333322, Velocity)
        ] |> Map.ofList

type USGSReading =
    {
        Site : string;
        Time : DateTime;
        Temperature : float;
        Discharge : float;
        GageHeight : float;
        Velocity : float
    }

type USGSReading with
    static member empty siteName =
        {Site = siteName; Time = DateTime.Now; Temperature = 0.0; Discharge = 0.0; GageHeight = 0.0; Velocity = 0.0}
    static member toString reading =
        [
            String.Format ("Site: {0} at {1}", reading.Site, reading.Time.ToString "MM/dd/yyyy h:mm tt");
            "----------------------------------------";
            String.Format ("Temperature: {0} fahrenheit degrees", reading.Temperature.ToString "F1");
            String.Format ("Discharge: {0} cubic feet per second", reading.Discharge.ToString "F1");
            String.Format ("Gage Height: {0} feet", reading.GageHeight.ToString "F2");
            String.Format ("Velocity: {0} feet per second", reading.Velocity.ToString "F2");
        ]
        |> String.concat Environment.NewLine

type USGSClient() =
    member this.Uri = USGS.mississippiAtStPaulUri
    member this.SiteName = USGS.mississippiAtStPaulSiteName
    member this.Retrieve () =
        let usgsResponse = USGSResponse.Load(this.Uri)
        let mutable usgsReading = USGSReading.empty this.SiteName
        for series in usgsResponse.Value.TimeSeries do
            if series.Variable.VariableCode.Length = 1 then
                let variableId = (Array.item 0 series.Variable.VariableCode).VariableId
                let readingValue = (float)(Array.last (Array.last series.Values).Value).Value
                let readingType = USGS.variableIdMap.Item variableId
                usgsReading <-
                    match readingType with
                    | Temperature -> {usgsReading with Temperature = readingValue}
                    | Discharge -> {usgsReading with Discharge = readingValue}
                    | GageHeight -> {usgsReading with GageHeight = readingValue}
                    | Velocity -> {usgsReading with Velocity = readingValue}
        usgsReading
