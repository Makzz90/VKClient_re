using Microsoft.Phone.Net.NetworkInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VKClient.Common.Framework;
using VKClient.Common.Utils;
using Windows.Networking.Connectivity;

namespace VKClient.Common.Library
{
    public class NetworkStatusInfo
    {
        private static NetworkStatusInfo _instance;
        private NetworkStatus _networkStatus;

        public static NetworkStatusInfo Instance
        {
            get
            {
                if (NetworkStatusInfo._instance == null)
                    NetworkStatusInfo._instance = new NetworkStatusInfo();
                return NetworkStatusInfo._instance;
            }
        }

        public NetworkStatus NetworkStatus
        {
            get
            {
                return this._networkStatus;
            }
            set
            {
                if (this._networkStatus == value)
                    return;
                this._networkStatus = value;
                EventAggregator.Current.Publish(new NetworkStatusChanged());
            }
        }

        public NetworkStatusInfo()
        {
            this.RetrieveNetworkStatus();
            Windows.Networking.Connectivity.NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            this.RetrieveNetworkStatus();
        }

        public void RetrieveNetworkStatus()
        {
            try
            {
                ConnectionProfile connectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
                if (connectionProfile != null)
                {
                    switch (connectionProfile.NetworkAdapter.IanaInterfaceType)
                    {
                        case 71:
                        case 6:
                            this.NetworkStatus = NetworkStatus.WiFi;
                            break;
                        default:
                            if (connectionProfile.GetConnectionCost().ApproachingDataLimit || connectionProfile.GetConnectionCost().OverDataLimit || connectionProfile.GetConnectionCost().Roaming)
                            {
                                this.NetworkStatus = NetworkStatus.MobileRestricted;
                                break;
                            }
                            this.NetworkStatus = NetworkStatus.MobileUnrestricted;
                            break;
                    }
                }
                else
                    this.NetworkStatus = NetworkStatus.None;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Cannot retrieve network status", ex);
            }
        }

        public ConnectionType RetrieveNetworkConnectionType()
        {
            List<NetworkInterfaceInfo> list1 = ((IEnumerable<NetworkInterfaceInfo>)new NetworkInterfaceList()).Where<NetworkInterfaceInfo>((Func<NetworkInterfaceInfo, bool>)(i => i.InterfaceState == ConnectState.Connected)).ToList<NetworkInterfaceInfo>();
            List<NetworkInterfaceType> list2 = ((IEnumerable<NetworkInterfaceInfo>)list1).Select<NetworkInterfaceInfo, NetworkInterfaceType>((Func<NetworkInterfaceInfo, NetworkInterfaceType>)(i => i.InterfaceType)).ToList<NetworkInterfaceType>();
            List<NetworkInterfaceSubType> list3 = ((IEnumerable<NetworkInterfaceInfo>)list1).Select<NetworkInterfaceInfo, NetworkInterfaceSubType>((Func<NetworkInterfaceInfo, NetworkInterfaceSubType>)(i => i.InterfaceSubtype)).ToList<NetworkInterfaceSubType>();
            string type = "unknown";
            string subtype = "unknown";
            if (list2.Contains((NetworkInterfaceType)71))
                type = "wifi";
            else if (list2.Contains((NetworkInterfaceType)145) || list2.Contains((NetworkInterfaceType)146))
            {
                type = "mobile";
                using (List<NetworkInterfaceSubType>.Enumerator enumerator = list3.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        switch (enumerator.Current - 1)
                        {
                            case NetworkInterfaceSubType.Cellular_GPRS:
                                subtype = "GPRS";
                                continue;
                            case NetworkInterfaceSubType.Cellular_1XRTT:
                                subtype = "1xRTT";
                                continue;
                            case NetworkInterfaceSubType.Cellular_EDGE:
                                subtype = "EDGE";
                                continue;
                            case NetworkInterfaceSubType.Cellular_3G:
                                subtype = "UMTS";
                                continue;
                            case NetworkInterfaceSubType.Cellular_HSPA:
                                subtype = "HSPA";
                                continue;
                            case NetworkInterfaceSubType.Cellular_LTE:
                                subtype = "LTE";
                                continue;
                            case NetworkInterfaceSubType.Cellular_EHRPD:
                                subtype = "EHRPD";
                                continue;
                            default:
                                continue;
                        }
                    }
                }
            }
            return new ConnectionType(type, subtype);
        }
    }
}
