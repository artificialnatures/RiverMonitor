open System
open System.Net
open RiverMonitor

[<EntryPoint>]
let main argv =
    let title = "River Monitor"
    Console.WriteLine title
    let usgsUri = "https://waterservices.usgs.gov/nwis/iv/?sites=05331000&period=P1D&format=json"
    let webClient = new WebClient()
    let retrievedString = webClient.DownloadString usgsUri
    let usgsResponse = USGSResponse.Parse retrievedString
    let dataTypeCount = usgsResponse.Value.TimeSeries.Length
    Console.WriteLine ("{0} types of data available.", dataTypeCount)
    for series in usgsResponse.Value.TimeSeries do
        Console.WriteLine series.Variable.VariableDescription
        let latestReading = Array.last (Array.last series.Values).Value
        Console.WriteLine ("Latest value: {0}", latestReading.Value)
        Console.WriteLine ("At: {0}", latestReading.DateTime)
    Console.WriteLine "Done."
    0
