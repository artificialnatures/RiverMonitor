namespace RiverMonitor

module Hexadecimal =
    let characters = ['0'; '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; 'A'; 'B'; 'C'; 'D'; 'E'; 'F']
    let convert (integer : int) =
        let sixteens = System.Math.Floor((float)integer / 16.0)
        let units = (float)integer - (sixteens * 16.0)
        [characters.[(int) sixteens]; characters.[(int) units]]
