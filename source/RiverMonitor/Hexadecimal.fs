namespace RiverMonitor

module Hexadecimal =
    let characters = ['0'; '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; 'A'; 'B'; 'C'; 'D'; 'E'; 'F']
    let characterIntegerMap =
        List.mapi (fun index character -> (character, index)) characters
        |> Map.ofList
    let toCharacters integer =
        let sixteens = System.Math.Floor((float)integer / 16.0)
        let units = (float)integer - (sixteens * 16.0)
        [characters.[(int) sixteens]; characters.[(int) units]]
    let characterToInteger index character =
        let integerValue = Map.find character characterIntegerMap
        let multiplier = if index = 0 then 1 else index * 16
        integerValue * multiplier
    let fromCharacters (characters : char list) =
        List.rev characters
        |> List.mapi characterToInteger
        |> List.sum
    let toBytes characters =
        List.toArray characters
        |> System.Text.Encoding.UTF8.GetBytes
    let fromBytes bytes =
        System.Text.Encoding.UTF8.GetChars bytes
        |> List.ofArray
