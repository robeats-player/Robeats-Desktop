using System.Windows.Controls;

namespace Robeats_Desktop.DataTypes
{
    public class Song
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Duration { get; set; }
        public string Hash { get; set; }

        public Song(string title, string artist, string duration, string hash)
        {
            Title = title;
            Artist = artist;
            Duration = duration;
            Hash = hash;
        }
    }
}