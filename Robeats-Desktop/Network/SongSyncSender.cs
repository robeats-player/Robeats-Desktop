using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Robeats_Desktop.Network
{
    class SongSyncSender
    {
        public TcpClient Client { get; set; }

        public SongSyncSender(IPEndPoint targetIp)
        {
            //Create a tcp listener on the local host on port 4568
            Client = new TcpClient();
            Client.Connect(targetIp);
        }

        /// <summary>
        /// Sync songs across devices
        /// </summary>
        public void Sync(List<byte[]> songHashes)
        {
            var stream = Client.GetStream();
            foreach (var songHash in songHashes)
            {
                var bytes = songHash;
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}