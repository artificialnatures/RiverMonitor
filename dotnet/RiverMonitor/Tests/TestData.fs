module TestData
    open System
    open RiverMonitor
    
    let temperatureToken = "{{rm-temperature}}"
    let dischargeVolumeToken = "{{rm-dischargevolume}}"
    let gageHeightToken = "{{rm-gageheight}}"
    let elevationToken = "{{rm-elevation}}"
    let velocityToken = "{{rm-velocity}}"
    let valueToken = "{{value}}"
    let timeToken = "{{time}}"
    let terminationToken = "{{termination}}"
    let valueTemplate =
        """
                            {
                                "value": "{{value}}",
                                "qualifiers": [
                                    "P"
                                ],
                                "dateTime": "{{time}}"
                            }{{termination}}
        """
    let replace (token : string) (replacement : string) (text : string) =
        text.Replace(token, replacement)
    let buildVariableJson (values : DateTime * float * bool) =
        let time, value, withComma = values
        let termination = if withComma then "," else ""
        valueTemplate
        |> replace valueToken (value.ToString "F1")
        |> replace timeToken (time.ToString "o")
        |> replace terminationToken termination
    let randomValuesForTimes lowerValueBound upperValueBound times =
        let generator = Random()
        let randomValue () =
            (generator.NextDouble() * (upperValueBound - lowerValueBound)) + lowerValueBound
        List.map (fun time -> (time, randomValue ())) times
    let generateTimes () =
        let now = DateTime.Now
        [-150.0 .. 15.0 .. 0.0]
        |> List.map TimeSpan.FromMinutes
        |> List.map (fun offset -> now.Add offset)
    let addTermination values =
        let lastIndex = List.length values - 1
        let findTermination index =
            if index = lastIndex then false else true
        [0..lastIndex]
        |> List.map findTermination
        |> List.zip values
        |> List.map (fun ((time, bounds), termination) -> (time, bounds, termination))
    let buildValueListJson lowerBound upperBound times =
        randomValuesForTimes lowerBound upperBound times
        |> addTermination
        |> List.map buildVariableJson
        |> List.toArray
        |> (fun json -> String.Join("", json))
    let buildJson level =
        let times = generateTimes ()
        let temperatureJson = buildValueListJson 32.0 99.0 times
        let lowerBound, upperBound = USGSWaterServices.findBoundsForDischargeLevel level
        let dischargeJson = buildValueListJson lowerBound upperBound times
        let gageHeightJson = buildValueListJson 0.0 50.0 times
        let elevationJson = buildValueListJson 650.0 700.0 times
        let velocityJson = buildValueListJson 0.3 30.0 times
        System.IO.File.ReadAllText(System.IO.Path.Combine("json", "USGSWaterServices_Template.json"))
        |> replace temperatureToken temperatureJson
        |> replace dischargeVolumeToken dischargeJson
        |> replace gageHeightToken gageHeightJson
        |> replace elevationToken elevationJson
        |> replace velocityToken velocityJson
