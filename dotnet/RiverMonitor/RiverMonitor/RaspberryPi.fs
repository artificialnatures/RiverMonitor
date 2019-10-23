namespace RiverMonitor

open System.Device.Gpio

type RaspberryPi() =
    let gpioController = new GpioController()
    member this.Dispose() = gpioController.Dispose()
