using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom.Events;

namespace Robeats_Desktop.Network
{
    class SongSyncListener
    {
        public event EventHandler DataReceived;
        public TcpListener Listener { get; set; }

        public SongSyncListener()
        {
            //Create a tcp listener on the local host on port 4568
            Listener = new TcpListener( IPAddress.Parse("127.0.0.1"), 4568);

        }
        /// <summary>
        /// Start listening 
        /// </summary>
        public void Listen()
        {
            Listener.Start();
            while (true)
            {
                var client = Listener.AcceptTcpClient();
                Debug.WriteLine("Connection established.");
                var stream = client.GetStream();
                if (stream.DataAvailable)
                {
                    var bytes = new byte[255];
                    stream.Read(bytes, 0, bytes.Length);
                    Debug.WriteLine(bytes.ToString());
                }
            }
        }

        private void OnDataReceived(EventArgs e)
        {
            DataReceived?.Invoke(this,e);
        }
    }
}
