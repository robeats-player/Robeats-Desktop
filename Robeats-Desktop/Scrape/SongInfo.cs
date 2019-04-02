using AngleSharp.Parser.Html;
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
        public string Url { get; set; }

        public SongInfo()
        {

        }

        public SongInfo(string url) {
            Url = url;
        }

        //TODO fix this mess
        private Dictionary<string, string> Get()
        {
            WebClient webClient = new WebClient();
            string s = webClient.DownloadString(Url);
            Debug.WriteLine(s);            

            var parser = new HtmlParser();

            var document = parser.Parse(s);

            var info2 = document.QuerySelector("#watch-description-content");
            var headers = info2.QuerySelectorAll("h4");
            var items = info2.QuerySelectorAll(".watch-info-tag-list");
            return headers.Zip(items, (k, v) => new { Key = k.TextContent.Trim(), Value = v.TextContent.Trim() })
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public void SetTags(TagLib.File libFile)
        {
            var dictionary = Get();
            foreach (var item in dictionary)
            {
                switch (item.Key.ToLower())
                {
                    case "song":
                        libFile.Tag.Title = item.Value;
                        break;
                    case "album":
                        libFile.Tag.Album = item.Value;
                        break;
                    case "artist":
                        libFile.Tag.Performers = new[] { item.Value };
                        break;
                    default:
                        break;
                }
            }
            libFile.Save();
        }
    }
}
