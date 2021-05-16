namespace RiverMonitor

type ConnectionCondition =
        | Normal
        | Troubled
        | Failed

module ConnectionCondition =
    let toString condition =
        match condition with
        | Normal -> "Normal"
        | Troubled -> "Troubled"
        | Failed -> "Failed"

type DeviceMode =
    | Live
    | Testing

type Device =
    abstract member Mode : DeviceMode
    abstract member IsConnected : bool
    abstract member Connect : unit -> unit
    abstract member DisplayCondition : ConnectionCondition -> unit
    abstract member SendRequest : ColorKinetics.Request -> Result<ColorKinetics.Response, string>
