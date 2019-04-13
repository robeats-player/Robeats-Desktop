using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AngleSharp;
using Robeats_Desktop.DataTypes;
using YoutubeExplode;
using YoutubeExplode.Models;
using Playlist = YoutubeExplode.Models.Playlist;

namespace Robeats_Desktop.Util
{
    public static class Download
    {
        /// <summary>
        /// Get the web video async
        /// </summary>
        /// <param name="videoUrl"></param>
        /// <returns>return a <see cref="Task{TResult}"/></returns>
        public static async Task<Video> GetVideoAsync(string videoUrl)
        {
            var id = YoutubeClient.ParseVideoId(videoUrl);
            var client = new YoutubeClient();
            var result = await client.GetVideoAsync(id);
            return result;
        }

        public static async Task<Playlist> GetPlaylistAsync(string playlistUrl)
        {
            try
            {
                var id = YoutubeClient.ParsePlaylistId(playlistUrl);
                var client = new YoutubeClient();
                return await client.GetPlaylistAsync(id);
            }
            catch (FormatException ex)
            {
                Debug.WriteLine(ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Download the playlist to the music folder.
        /// </summary>
        /// <param name="downloadQueue">The <see cref="DownloadQueue"/> Collection you want to download</param>
        /// <param name="parallelDownloads">The amount of parallel downloads.</param>
        public static void DownloadPlaylist(DownloadQueue downloadQueue, int parallelDownloads)
        {
            while (downloadQueue.HasNext())
            {
                downloadQueue.DownloadNext(parallelDownloads);
            }
        }

        public static async void DownloadSong(string videoUrl)
        {
            var video = await GetVideoAsync(videoUrl);
            DownloadSong(new DownloadQueue {new DownloadItem(video)});
        }

        public static void DownloadSong(DownloadQueue downloadQueue)
        {
            downloadQueue.DownloadNext(1);
        }
    }
}