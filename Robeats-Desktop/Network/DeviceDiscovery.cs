using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Robeats_Desktop.Network.Frames;

namespace Robeats_Desktop.Network
{
    class DeviceDiscovery
    {
        public IPEndPoint MulticastEndPoint { get; set; }
        public bool Visible { get; set; }
        public static readonly HashSet<RobeatsDevice> RobeatsDevices;

        static DeviceDiscovery()
        {
            RobeatsDevices = new HashSet<RobeatsDevice>();
        }


        public DeviceDiscovery() : this(new IPEndPoint(IPAddress.Parse("224.5.6.7"), 4567))
        {
        }

        public DeviceDiscovery(IPEndPoint multicastEndPoint)
        {
            MulticastEndPoint = multicastEndPoint;
            Visible = true;
        }

        /// <summary>
        /// Send a request to the multicast address.
        /// </summary>
        /// <param name="requestType"></param>
        public void SendRequest(ProtocolRequest requestType)
        {
            SendRequest(requestType, MulticastEndPoint, 0, 0);
        }

        /// <summary>
        /// Send a request to a specific <see cref="IPEndPoint"/>
        /// </summary>
        /// <param name="requestType"></param>
        /// <param name="endPoint"></param>
        /// <param name="retries"></param>
        /// <param name="interval"></param>
        public void SendRequest(ProtocolRequest requestType, IPEndPoint endPoint, int retries, int interval)
        {
            if (!Visible) return;
            var udpHelper = new UdpHelper(endPoint);

            for (var i = 0; i < retries + 1; i++)
            {
                //TODO implement device names
                var stateProtocol = new StateProtocol(requestType, "SomeName", 255);
                udpHelper.Send(stateProtocol.ToBytes());
                Thread.Sleep(interval);
            }

            udpHelper.Close();
        }

        public HashSet<RobeatsDevice> AwaitResponses(int timeout)
        {
            Stopwatch stopwatch = new Stopwatch();
            try
            {
                using (var client = new UdpClient(4567, AddressFamily.InterNetwork))
                {
                    client.Ttl = 2;
                    client.Client.ReceiveTimeout = timeout;
                    client.JoinMulticastGroup(MulticastEndPoint.Address);
                    client.Client.ReceiveBufferSize = 18;
                    var ipEndPoint = new IPEndPoint(IPAddress.Any, MulticastEndPoint.Port);
                    stopwatch.Start();
                    while (stopwatch.ElapsedMilliseconds < timeout)
                    {
                        var bytes = client.Receive(ref ipEndPoint);

                        var stateProtocol = StateProtocol.FromBytes(bytes);

                        //Check if reply is an actual discovery reply
                        if (stateProtocol.ProtocolType == ProtocolRequest.DeviceDiscoveryReply)
                        {
                            var robeatsDevice = new RobeatsDevice
                            {
                                Id = stateProtocol.DeviceId,
                                Name = stateProtocol.DeviceName,
                                EndPoint = ipEndPoint
                            };
                            RobeatsDevices.Add(robeatsDevice);
                        }
                        else
                        {
                            Debug.WriteLine("Not a reply. ignoring");
                        }
                    }

                    stopwatch.Stop();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                stopwatch.Stop();
            }

            return RobeatsDevices;
        }


        public void SendDiscoveryReply(RobeatsDevice device)
        {
            using (var client = new UdpClient(4567, AddressFamily.InterNetwork))
            {
                client.Ttl = 2;
                client.JoinMulticastGroup(MulticastEndPoint.Address);
                client.Client.ReceiveBufferSize = 18;
                var ipEndPoint = new IPEndPoint(IPAddress.Any, MulticastEndPoint.Port);

                var bytes = client.Receive(ref ipEndPoint);

                var stateProtocol = StateProtocol.FromBytes(bytes);

                //Check if reply is an actual discovery reply
                if (stateProtocol.ProtocolType == ProtocolRequest.DeviceDiscovery)
                {
                    var robeatsDevice = new RobeatsDevice
                    {
                        Id = stateProtocol.DeviceId,
                        Name = stateProtocol.DeviceName,
                        EndPoint = ipEndPoint
                    };
                    RobeatsDevices.Add(robeatsDevice);
                    var stateProtocolReply = new StateProtocol(ProtocolRequest.DeviceDiscoveryReply, device.Name, device.Id);
                    var bytesReply = stateProtocolReply.ToBytes();
                    client.Close();
                    var clientReply = new UdpClient(4567) {Ttl = 2};
                    clientReply.Connect(ipEndPoint);
                    clientReply.Send(bytesReply, bytes.Length);
                    clientReply.Close();
                    Debug.WriteLine(ipEndPoint.Address);
                }
                else
                {
                    Debug.WriteLine("Not a discoveryRequest. ignoring");
                }
            }
        }
    }
}