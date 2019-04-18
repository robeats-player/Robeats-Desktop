using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Robeats_Desktop.Annotations;

namespace Robeats_Desktop.Network
{
    class UdpHelper : UdpClient
    {

        public IPEndPoint IpEndPoint { get; set; }

        public UdpHelper([NotNull] string hostname, int port) : this(new IPEndPoint(IPAddress.Parse(hostname), port))
        {

        }
        public UdpHelper([NotNull] IPEndPoint ipEndPoint) : base(AddressFamily.InterNetworkV6)
        {
            IpEndPoint = ipEndPoint;
            Connect(IpEndPoint);
        }

        public void Send(byte[] data)
        {
            Send(data, data.Length);
        }
    }
}
