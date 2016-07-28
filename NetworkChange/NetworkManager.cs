using NETCONLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkChange
{
    public class NetworkManager
    {
        public static void Control(bool isConnect)
        {
            NetSharingManagerClass netSharingMgr = new NetSharingManagerClass();
            INetSharingEveryConnectionCollection connections = netSharingMgr.EnumEveryConnection;
            foreach (INetConnection connection in connections)
            {
                INetConnectionProps connProps = netSharingMgr.get_NetConnectionProps(connection);
                if (connProps.MediaType == tagNETCON_MEDIATYPE.NCM_LAN)
                {
                    if (isConnect)
                    {
                        connection.Connect(); //启用网络
                    }
                    else
                    {
                        connection.Disconnect(); //禁用网络
                    }
                }
            }
        }
    }
}
