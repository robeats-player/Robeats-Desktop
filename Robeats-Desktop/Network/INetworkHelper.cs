﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robeats_Desktop.Network
{
    interface INetworkHelper
    {
        void Send(byte[] data);
        byte[] Receive();
    }
}
