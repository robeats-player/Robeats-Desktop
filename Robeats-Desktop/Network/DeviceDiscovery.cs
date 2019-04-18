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
        public event EventHandler<DeviceDiscoveryEventArgs> DeviceDiscoverReply;

        public IPEndPoint MulticastEndPoint { get; set; }
        public bool Visible { get; set; }

        //ipv4 = 224.5.6.7
        //ipv6 = FF02::5:6:7
        public DeviceDiscovery() : this(new IPEndPoint(IPAddress.Parse("224.5.6.7"), 4567))
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
                var ipEndPoint = new IPEndPoint(((IPEndPoint)robeatsDevice.EndPoint).Address, 4568);
                udpHelper = new UdpHelper(ipEndPoint);
            }

            var stateProtocol = new StateProtocol(requestType, robeatsDevice.Name, robeatsDevice.Id);
            Debug.WriteLine("Sending request of type: "+requestType);
            udpHelper.Send(stateProtocol.ToBytes());
            udpHelper.Close();
        }


        public async void AwaitMulticastRequest()
        {
            try
            {
                //Listen on port 4568 since its Unicast and cant be bound to port 4567 (in use for multicast)
                using (var client = new UdpClient(MulticastEndPoint.Port, AddressFamily.InterNetwork))
                {
                    client.Ttl = 2;
                    client.JoinMulticastGroup(MulticastEndPoint.Address);
                    client.Client.ReceiveBufferSize = 18;
                    var ipEndPoint = new IPEndPoint(IPAddress.Loopback, MulticastEndPoint.Port);
                    while (true)
                    {
                        Debug.WriteLine("Awaiting multicast request");
                        var result = await client.ReceiveAsync();

                        var stateProtocol = StateProtocol.FromBytes(result.Buffer);

                        //Check if reply is an actual discovery reply
                        if (stateProtocol.ProtocolType == ProtocolRequest.DeviceDiscovery)
                        {
                            //TODO ignore own requests
                            if (Equals(ipEndPoint.Address, Localhost.GetIpAddress(Dns.GetHostName()))) return;
                            var robeatsDevice = new RobeatsDevice
                            {
                                Id = stateProtocol.DeviceId,
                                Name = stateProtocol.DeviceName,
                                EndPoint = new IPEndPoint(ipEndPoint.Address,4568),
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
                var clientReply = new UdpClient(AddressFamily.InterNetwork) {Ttl = 2};
                Debug.WriteLine($"Sending DR: {device.EndPoint}");
                clientReply.Send(bytesReply, bytesReply.Length, (IPEndPoint)device.EndPoint);
                clientReply.Close();
            }
            else
            {
                Debug.WriteLine("Not a discoveryRequest. ignoring");
            }
        }


        public async void AwaitDiscoveryReply()
        {
            try
            {
                using (var client = new UdpClient(4568, AddressFamily.InterNetwork))
                {
                    while (true)
                    {
                        Debug.WriteLine("Awaiting DR");

                        var result = await client.ReceiveAsync();
                        Debug.WriteLine("Received DR");
                        var stateProtocol = StateProtocol.FromBytes(result.Buffer);

                        if (stateProtocol.ProtocolType == ProtocolRequest.DeviceDiscoveryReply)
                        {
                            var robeatsDevice = new RobeatsDevice
                            {
                                Id = stateProtocol.DeviceId,
                                Name = stateProtocol.DeviceName,
                                /*EndPoint = client.Client.RemoteEndPoint,*/
                                StateProtocol = stateProtocol

                            };
                            OnDiscoveryReply(new DeviceDiscoveryEventArgs { RobeatsDevice = robeatsDevice });
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        protected virtual void OnDeviceDetect(DeviceDiscoveryEventArgs args)
        {
            DeviceDetect?.Invoke(this, args);
        }

        protected virtual void OnDiscoveryReply(DeviceDiscoveryEventArgs args)
        {
            DeviceDiscoverReply?.Invoke(this, args);
        }
    }
}