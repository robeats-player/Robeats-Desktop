using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using Robeats_Desktop.DataTypes;
using YoutubeExplode.Models;

namespace Robeats_Desktop.Parsers
{
    public class SongDetailParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns><see cref="HashSet{T}"/></returns>
        public static Song Get(DownloadItem item)
        {
            try
            {
                var webClient = new WebClient();
                var downloadString = webClient.DownloadString(item.Video.GetUrl());

                var parser = new HtmlParser();

                var document = parser.Parse(downloadString);

                var htmlContent = document.QuerySelector("#watch-description-content");
                var htmlHeaders = htmlContent.QuerySelectorAll("h4");
                var htmlTags = htmlContent.QuerySelectorAll(".watch-info-tag-list");

                var song = new Song();
                for (var i = 0; i < htmlHeaders.Length; i++)
                {
                    var header = htmlHeaders[i].TextContent.Trim();
                    if (header.Equals("Song"))
                    {
                        song.Title = htmlTags[i].TextContent.Trim();
                    }else if (header.Equals("Artist"))
                    {
                        song.Artist = htmlTags[i].TextContent.Trim();

                    }
                    else if(header.Equals("Album"))
                    {
                        song.Album = htmlTags[i].TextContent.Trim();
                    }
                }
                Debug.WriteLine(song);
                return song;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}
