namespace RiverMonitor.App

module Network =
    type WifiNetwork =
        {
            SSID : string
            Password : string
        }

    let approvedWifiNetworks =
        [
            {SSID = "Corbu2600"; Password = "singular"}
        ]
    
    let isApproved (availableNetworkNames : string list) (approvedNetwork : WifiNetwork) =
        List.exists (fun networkName -> networkName = approvedNetwork.SSID) availableNetworkNames
    
    let findApprovedNetwork availableNetworkNames =
        let whereApproved = isApproved availableNetworkNames
        List.tryFind whereApproved approvedWifiNetworks
