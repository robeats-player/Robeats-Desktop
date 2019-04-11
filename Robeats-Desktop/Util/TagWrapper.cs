using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Media.Imaging;
using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using Robeats_Desktop.DataTypes;
using TagLib;

namespace Robeats_Desktop.Util
{
    enum RequestType
    {
        Song, Album
    }
    public class TagWrapper
    {
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Track { get; set; }
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

        private async Task<Bitmap> GetAlbumCoverAsync(Uri url, RequestType requestType)
        {
            var client = new HttpClient();
            var jsonString = await client.GetStringAsync(url);

            switch (requestType)
            {
                case RequestType.Album:
                    var albumJson = JsonConvert.DeserializeObject<AlbumJson>(jsonString);
                    return Thumbnail.DownloadImage(albumJson.Album.Image[4].Text.AbsoluteUri);
                case RequestType.Song:
                    var tracks = JsonConvert.DeserializeObject<Tracks>(jsonString);
                    //return Thumbnail.DownloadImage(tracks.Track[0]);
                    break;
            }

            return null;

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
                var dic = htmlHeaders.Zip(htmlTags,
                        (k, v) => new
                        {
                            Key = k.TextContent.Trim(),
                            Value = Regex.Replace(v.TextContent.Trim(), "\\((.*?)\\)", "").Trim()
                        })
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
                        Track = item.Value;
                        break;
                    case "album":
                        TagFile.Tag.Album = item.Value;
                        Album = item.Value;
                        break;
                    case "artist":
                        TagFile.Tag.Performers = new[] {item.Value};
                        Artist = item.Value;
                        break;
                    default: break;
                }
            }

            TagFile.Save();
        }

        public async void SetCover()
        {
            if (Artist == null && Album == null) return;
            StringBuilder sb;
            if (Album == null && Track != null)
            {
                sb = new StringBuilder(
                    "https://audioscrobbler.com/2.0/?method=track.getInfo&api_key=7791d9e8d9ba29503d69f5d27304de22&format=json");
                sb.Append($"&Artist={Artist}");
                sb.Append($"&track={Track}");
            }
            else
            {
                sb = new StringBuilder(
                    "https://audioscrobbler.com/2.0/?method=album.getinfo&api_key=7791d9e8d9ba29503d69f5d27304de22&format=json");
                if (!(Artist == null || Artist.Equals(string.Empty))) sb.Append($"&Artist={Artist}");
                if (!(Album == null || Album.Equals(string.Empty))) sb.Append($"&Album={Album}");
            }

            var link = sb.ToString().Replace(" ", "%20");
            var bitmap = await GetAlbumCoverAsync(new Uri(link), RequestType.Album);
            if (bitmap == null) return;
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            var pic = new Picture
            {
                Type = PictureType.FrontCover, Description = "Cover", Data = ByteVector.FromStream(ms)
            };

            TagFile.Tag.Pictures = new IPicture[] {pic};
            TagFile.Save();

            ms.Close();
        }
    }
}