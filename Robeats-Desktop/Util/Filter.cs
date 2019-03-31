using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Robeats_Desktop.Util
{
    class Filter
    {

        

        /// <summary>
        /// Get the artist from a string where the first part of the title is the artist.
        /// Example: The Artist - Some Song will return The Artist.
        /// </summary>
        /// <param name="fullSongName">The full song title</param>
        /// <returns>The artist</returns>
        /*public static Artist GetArtists(string fullSongName)
        {
            Artist artists = new Artist();
            artists.MainArtist = fullSongName.Split(new[] { '-' }, 2)[0].Trim();

        }*/

        /// <summary>
        /// Get the title from a string where the second part of the full title is the title.
        /// Example: The Artist - Some Song will return Some Song.
        /// </summary>
        /// <param name="fullSongName">The full song title</param>
        /// <returns>The artist</returns>
        public static string GetTitle(string fullSongName)
        {
            var regex = new Regex(@"(?<=\(f(?:ea)?t\. +)(.+)(?=\))");
            var title = fullSongName.Split(new[] { '-' }, 2)[1].Trim();
            var match = regex.Match(title);
            if (match.Success)
            {
                return title.Replace(match.Groups[1].Value, "");
            }

            return title;
        }
    }
}
