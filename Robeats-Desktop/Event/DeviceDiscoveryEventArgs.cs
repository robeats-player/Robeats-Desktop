using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robeats_Desktop.Network;

namespace Robeats_Desktop.Event
{
    class DeviceDiscoveryEventArgs : EventArgs
    {
        public RobeatsDevice RobeatsDevice { get; set; }
    }
}
