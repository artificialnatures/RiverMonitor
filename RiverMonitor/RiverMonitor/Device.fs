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

type Device =
    abstract member SendRequest : ColorKinetics.Request -> Result<ColorKinetics.Response, string>
