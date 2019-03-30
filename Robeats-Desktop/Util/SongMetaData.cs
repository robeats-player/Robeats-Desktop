using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robeats_Desktop.Util
{
    public class SongMetaData
    {
        private string _path;

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                TagFile = TagLib.File.Create(Path);
            }
        }


        public TagLib.File TagFile { get; set; }

        public SongMetaData()
        {
        }

        public SongMetaData(string path)
        {
            Path = path;
        }

        public void AddTitle(string title)
        {
            TagFile.Tag.Title = title;
        }

        public void AddArtists(string[] artists)
        {
            TagFile.Tag.Performers = artists;
        }
    }
}