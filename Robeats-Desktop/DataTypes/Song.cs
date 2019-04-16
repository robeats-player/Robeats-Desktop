using System.Collections.Generic;
using System.Windows.Controls;
using TagLib.Riff;

namespace Robeats_Desktop.DataTypes
{
    public class Song
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Duration { get; set; }
        public string Hash { get; set; }
        public string AbsolutePath { get; set; }
        public string Album { get; set; }
        public Song()
        {
        }

        public Song(string title, string artist, string duration, string absolutePath, string hash) : this(title,
            artist, duration, absolutePath)
        {
            Hash = hash;
        }

        public Song(string title, string artist, string duration)
        {
            Title = title;
            Artist = artist;
            Duration = duration;
        }

        public Song(string title, string artist, string duration, string absolutePath) : this(title, artist, duration)
        {
            AbsolutePath = absolutePath;
        }

        public override string ToString()
        {
            return
                $"Title:{Title}, Artist:{Artist}, Duration:{Duration}, Hash:{Hash}, AbsolutePath:{AbsolutePath}, Album:{Album}";
        }

        /*public static List<Song> GetSongs()
        {

        }*/
    }
}