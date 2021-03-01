namespace RiverMonitor

open Meadow
open Meadow.Devices
open Meadow.Foundation.Leds
open ApplicationState

//Interface with the Meadow F7 microcontroller by Wilderness Labs
//Refer to: http://developer.wildernesslabs.co/

type MeadowDevice() =
    inherit App<F7Micro, MeadowDevice>()
    let redPin = MeadowDevice.Device.CreateDigitalOutputPort MeadowDevice.Device.Pins.OnboardLedRed
    let greenPin = MeadowDevice.Device.CreateDigitalOutputPort MeadowDevice.Device.Pins.OnboardLedGreen
    let bluePin = MeadowDevice.Device.CreateDigitalOutputPort MeadowDevice.Device.Pins.OnboardLedBlue
    let led = RgbLed(redPin, greenPin, bluePin)
    
    interface Device with
        member this.DisplayCondition condition =
            match condition with
            | Normal -> led.SetColor RgbLed.Colors.Green
            | Troubled -> led.SetColor RgbLed.Colors.Yellow
            | Failed -> led.SetColor RgbLed.Colors.Red
