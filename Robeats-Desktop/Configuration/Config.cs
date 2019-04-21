using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config.Net;

namespace Robeats_Desktop.Configuration
{
    public interface IConfig
    {
        byte Id { get; set; }

        [Option(DefaultValue = "Device1")]
        string DeviceName { get; set; }

    }
}
