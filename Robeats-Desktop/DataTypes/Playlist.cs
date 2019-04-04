using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Robeats_Desktop.Util;

namespace Robeats_Desktop.DataTypes
{
    class Playlist
    {
        //Name of the playlist
        public string Name { get; set; }

        //Store as MD5 for easy validation and transfer over network.
        public HashSet<string> SongHashSet { get; set; }

        /// <summary>
        /// Add a song hash to the playlist
        /// </summary>
        /// <param name="fileName">the absolute path to the file</param>
        public void AddSong(string fileName)
        {
            SongHashSet.Add(Md_5.Calculate(fileName));
        }
    }
}
