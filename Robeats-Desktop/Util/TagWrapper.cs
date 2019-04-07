using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;

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

        public string YoutubeUrl { get; set; }


        public TagLib.File TagFile { get; set; }

        public TagWrapper()
        {

        }

        public TagWrapper(string path, string youtubeUrl)
        {
            Path = path;
            YoutubeUrl = youtubeUrl;
        }


        /// <summary>
        /// Get the music details section from youtube.
        /// </summary>
        /// <returns>Returns a <see cref="Dictionary{TKey,TValue}"/> with the title as key and name as value</returns>
        private Dictionary<string, string> Get()
        {
            try
            {
                var webClient = new WebClient();
                var downloadString = webClient.DownloadString(YoutubeUrl);

                var parser = new HtmlParser();

                var document = parser.Parse(downloadString);

                var htmlContent = document.QuerySelector("#watch-description-content");
                var htmlHeaders = htmlContent.QuerySelectorAll("h4");
                var htmlTags = htmlContent.QuerySelectorAll(".watch-info-tag-list");
                var dic = htmlHeaders.Zip(htmlTags, (k, v) => new { Key = k.TextContent.Trim(), Value = v.TextContent.Trim() })
                    .ToDictionary(x => x.Key, x => x.Value);
                foreach (var keyValuePair in dic)
                {
                    Debug.WriteLine(keyValuePair.Key + "//" + keyValuePair.Value);
                }
                return dic;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        /// <summary>
        /// Set the tags to the actual file
        /// </summary>
        public void SetTags()
        {
            var dictionary = Get();
            foreach (var item in dictionary)
            {
                switch (item.Key.ToLower())
                {
                    case "song":
                        TagFile.Tag.Title = item.Value;
                        break;
                    case "album":
                        TagFile.Tag.Album = item.Value;
                        break;
                    case "artist":
                        TagFile.Tag.Performers = new[] { item.Value };
                        break;
                    default: break;
                }
            }

            TagFile.Save();
        }
    }
}