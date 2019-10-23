open RiverMonitor
open System
open System.Threading

[<EntryPoint>]
let main argv =
    Console.WriteLine "River Monitor"
    let usgsClient = USGSClient()
    let mutable running = true
    let mutable lastRetrievalAt = DateTime.Now - TimeSpan.FromHours 1.0
    let retrievalInterval = TimeSpan.FromMinutes 5.0
    let readKeyboardInput () =
        if Console.KeyAvailable then
            match Console.ReadKey().Key with
            | ConsoleKey.Escape -> running <- false
            | _ -> ()
        ()
    while running do
        if DateTime.Now > (lastRetrievalAt + retrievalInterval) then
            let reading = usgsClient.Retrieve()
            lastRetrievalAt <- reading.Time
            Console.WriteLine (USGSReading.toString reading)
        readKeyboardInput()
        Thread.Sleep 100
    Console.WriteLine "Done."
    0
