open System

type TimeSpan with
    member timeOfDay.isWithin openingTime closingTime =
        openingTime < timeOfDay && timeOfDay < closingTime

let openingTime = TimeSpan.Parse("06:00")
let closingTime = TimeSpan.Parse("18:00")

if DateTime.Now.TimeOfDay.isWithin openingTime closingTime
then Console.WriteLine "We're open!"
else Console.WriteLine $"Sorry we close at {closingTime}. We open again tomorrow at {openingTime}."
