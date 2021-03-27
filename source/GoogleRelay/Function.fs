namespace GoogleRelay

open Google.Cloud.Functions.Framework
open Google.Events.Protobuf.Cloud.PubSub.V1
open System.Threading.Tasks
open RiverMonitor
open RiverMonitor.ApplicationState

type Function() =
    interface ICloudEventFunction<MessagePublishedData> with
        member this.HandleAsync(cloudEvent, data, cancellationToken) =
            let initialState = Application.initialState
                                   ExecutionEnvironment.CommandLine
                                   ExecutionStrategy.RetrieveFromUSGS
                                   None
            let updatedState =
                Application.chooseMode initialState
                |> Application.verifyConnection
                |> Application.retrieveReading
                |> Application.updateLights
                |> Application.assessCondition
                |> Application.adjustPollInterval
                |> Application.displayCondition
            //Publish message with updated state...either:
            //1. publish USGSReading
            //2. publish event based on the evaluated Request
            Task.CompletedTask
