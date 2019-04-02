using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robeats_Desktop.Util
{
    public class TagWrapper
    {
        private string _path;

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                TagFile = TagLib.File.Create(value);
            }
        }


        public TagLib.File TagFile { get; set; }

        public TagWrapper()
        {
        }

        public TagWrapper(string path)
        {
            Path = path;
        }
    }
}