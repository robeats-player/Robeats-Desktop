using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Robeats_Desktop.Network
{
    class Localhost
    {
        public static IPAddress GetIpAddress(string hostName)
        {
            var ping = new Ping();
            var replay = ping.Send(hostName);

            if (replay != null && replay.Status == IPStatus.Success)
            {
                return replay.Address;
            }
            return null;
        }
    }
}
