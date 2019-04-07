using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Robeats_Desktop.Util;

namespace Robeats_Desktop.DataTypes
{
    public class Playlist
    {
        public string Name { get; set; }
        public HashSet<Song> Songs { get; set; }
    }
}
