namespace RiverMonitor

type ConnectionCondition =
        | Normal
        | Troubled
        | Failed

type DeviceMode =
    | Live
    | Testing

type Device =
    abstract member Mode : DeviceMode
    abstract member IsConnected : bool
    abstract member Connect : unit -> unit
    abstract member DisplayCondition : ConnectionCondition -> unit
