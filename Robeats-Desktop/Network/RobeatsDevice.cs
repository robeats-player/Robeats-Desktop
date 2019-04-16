using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Robeats_Desktop.Network.Frames;

namespace Robeats_Desktop.Network
{
    public class RobeatsDevice
    {
        public byte Id { get; set; }
        public string Name { get; set; }
        public bool Synced { get; set; }
        public EndPoint EndPoint { get; set; }
        public bool IsBeingUpdated { get; set; }
        public StateProtocol StateProtocol { get; set; }
    }
}
