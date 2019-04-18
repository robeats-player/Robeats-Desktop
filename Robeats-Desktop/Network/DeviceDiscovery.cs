using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Robeats_Desktop.Event;
using Robeats_Desktop.Network.Frames;

namespace Robeats_Desktop.Network
{
    class DeviceDiscovery
    {
        public event EventHandler<DeviceDiscoveryEventArgs> DeviceDetect;

        public IPEndPoint MulticastEndPoint { get; set; }
        public bool Visible { get; set; }

        //ipv4 = 224.5.6.7
        //ipv6 = FF02::5:6:7
        public DeviceDiscovery() : this(new IPEndPoint(IPAddress.Parse("FF02::5:6:7"), 4567))
        {
        }

        public DeviceDiscovery(IPEndPoint multicastEndPoint)
        {
            MulticastEndPoint = multicastEndPoint;
            Visible = true;
        }

        /// <summary>
        /// Send a request to a specific <see cref="IPEndPoint"/>
        /// </summary>
        /// <param name="requestType"></param>
        /// <param name="robeatsDevice"></param>
        public void SendRequest(ProtocolRequest requestType, RobeatsDevice robeatsDevice)
        {
            if (!Visible) return;
            UdpHelper udpHelper;
            if (requestType == ProtocolRequest.DeviceDiscovery)
            {
                udpHelper = new UdpHelper(MulticastEndPoint);
            }
            else
            {
                udpHelper = new UdpHelper((IPEndPoint)robeatsDevice.EndPoint);
            }

            var stateProtocol = new StateProtocol(requestType, robeatsDevice.Name, robeatsDevice.Id);
            udpHelper.Send(stateProtocol.ToBytes());
            udpHelper.Close();
        }


        public void AwaitResponse()
        {
            try
            {
                using (var client = new UdpClient(4567, AddressFamily.InterNetworkV6))
                {
                    client.Ttl = 2;
                    client.JoinMulticastGroup(MulticastEndPoint.Address);
                    client.Client.ReceiveBufferSize = 18;
                    var ipEndPoint = new IPEndPoint(IPAddress.Any, MulticastEndPoint.Port);
                    while (true)
                    {
                        var bytes = client.Receive(ref ipEndPoint);

                        var stateProtocol = StateProtocol.FromBytes(bytes);

                        //Check if reply is an actual discovery reply
                        if (stateProtocol.ProtocolType == ProtocolRequest.DeviceDiscovery)
                        {
                            //TODO ignore own requests
                            if (Equals(ipEndPoint.Address, Localhost.GetIpAddress(Dns.GetHostName()))) return;
                            var robeatsDevice = new RobeatsDevice
                            {
                                Id = stateProtocol.DeviceId,
                                Name = stateProtocol.DeviceName,
                                EndPoint = ipEndPoint,
                                StateProtocol = stateProtocol

                            };

                            OnDeviceDetect(new DeviceDiscoveryEventArgs {RobeatsDevice = robeatsDevice});
                        }
                        else
                        {
                            Debug.WriteLine("Not a reply. ignoring");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void SendDiscoveryReply(RobeatsDevice device)
        {
            //Check if reply is an actual discovery reply
            if (device.StateProtocol.ProtocolType == ProtocolRequest.DeviceDiscovery)
            {
                var stateProtocolReply =
                    new StateProtocol(ProtocolRequest.DeviceDiscoveryReply, device.Name, device.Id);
                var bytesReply = stateProtocolReply.ToBytes();
                var clientReply = new UdpClient(AddressFamily.InterNetworkV6) {Ttl = 2};
                clientReply.Connect((IPEndPoint) device.EndPoint);
                clientReply.Send(bytesReply, bytesReply.Length);
                clientReply.Close();
            }
            else
            {
                Debug.WriteLine("Not a discoveryRequest. ignoring");
            }
        }

        protected virtual void OnDeviceDetect(DeviceDiscoveryEventArgs args)
        {
            DeviceDetect?.Invoke(this, args);
        }
    }
}