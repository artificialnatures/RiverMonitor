namespace RiverMonitor

module Hours =
    type System.TimeSpan with
        member timeOfDay.isWithin openingTime closingTime =
            openingTime < timeOfDay && timeOfDay < closingTime
