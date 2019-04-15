using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Parser.Xml;
using Robeats_Desktop.DataTypes;

namespace Robeats_Desktop.Parsers
{
    class CoverParser
    {
        private const string BaseUrl = "http://ws.audioscrobbler.com/2.0/?method=track.getInfo&api_key=7791d9e8d9ba29503d69f5d27304de22";

        public string FullUrl { get; set; }

        public string SongName { get; set; }
        public string ArtistName { get; set; }

        public CoverParser(string songName, string artistName)
        {
            SongName = songName;
            ArtistName = artistName;
            FullUrl = $"{BaseUrl}&artist={ArtistName}&track={SongName}";
        }

        public async Task<Bitmap> GetAsync()
        {
            using (var client = new WebClient())
            {
                var text = await client.DownloadStringTaskAsync(FullUrl);
                var xmlParser = new XmlParser();
                var xml = xmlParser.Parse(text);
                var images = xml.GetElementsByTagName("image");
                if (images.Length > 0)
                {
                    var image = images[images.Length - 1];
                    return ImageHelper.GetAsBitmap(image.TextContent);
                }
            }

            return null;
        }
    }
}