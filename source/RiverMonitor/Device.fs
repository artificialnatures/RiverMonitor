namespace RiverMonitor

open RiverMonitor.ApplicationState

type Device =
    abstract member DisplayCondition : Condition -> unit
