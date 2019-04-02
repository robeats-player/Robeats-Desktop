using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models;

namespace Robeats_Desktop.Scrape
{
    public class VideoInfo
    {
        public YoutubeClient YoutubeClient { get; set; }
        public Video Video { get; set; }
        public string Url { get; set; }

        public VideoInfo()
        {
        }

        public VideoInfo(string url)
        {
            Url = url;
        }

        /// <summary>
        /// Get the music details section from youtube.
        /// </summary>
        /// <returns>Returns a <see cref="Dictionary{TKey,TValue}"/> with the title as key and name as value</returns>
        private Dictionary<string, string> Get()
        {
            var webClient = new WebClient();
            var downloadString = webClient.DownloadString(Url);
            Debug.WriteLine(downloadString);

            var parser = new HtmlParser();

            var document = parser.Parse(downloadString);

            var htmlContent = document.QuerySelector("#watch-description-content");
            var htmlHeaders = htmlContent.QuerySelectorAll("h4");
            var htmlTags = htmlContent.QuerySelectorAll(".watch-info-tag-list");
            return htmlHeaders.Zip(htmlTags, (k, v) => new {Key = k.TextContent.Trim(), Value = v.TextContent.Trim()})
                .ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Set the ID3 tags of the <see cref="libFile"/>.
        /// </summary>
        /// <param name="libFile"></param>
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
                        libFile.Tag.Performers = new[] {item.Value};
                        break;
                    default: break;
                }
            }

            libFile.Save();
        }
    }
}