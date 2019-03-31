using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Models;

namespace Robeats_Desktop.Scrape
{
    class SongInfo
    {
        public Video Video { get; set; }

        public string SongName { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Author { get; set; }

        //TODO fix this mess
        public void Get(string url)
        {
            Regex regex = new Regex("(<ul class=\"content watch-info-tag-list\">\\s*<li>\\s*(.+)s*</li>\\s*</ul>)");
            WebClient webClient = new WebClient();
            string s = webClient.DownloadString(url);
            var matches = regex.Matches(s);

            List<string> info = new List<string>();
            foreach (Match match in matches)
            {
                
                var result = match.Groups[2].Value;
                if (result.StartsWith("<a"))
                {
                    result = new Regex("<a.*>(.*)</a>").Match(result).Groups[1].Value;
                }
                Debug.WriteLine(result);
                info.Add(result);
                
            }

            SongName = info[1];
            Artist = info[2];
            Album = info[3];
            Author = info[4];
        }
    }
}
