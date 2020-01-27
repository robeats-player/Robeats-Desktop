using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.NET;
using Robeats_Desktop.Parsers;
using Robeats_Desktop.Util;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace Robeats_Desktop.DataTypes
{
    public class DownloadQueue : Queue<DownloadItem>
    {
        public List<Song> ProcessedSongs { get; set; }

        public DownloadQueue()
        {
            ProcessedSongs = new List<Song>();
        }

        public void Add(DownloadItem item)
        {
            Enqueue(item);
        }


        public bool HasNext()
        {
            return Count > 0;
        }

        public void DownloadNext()
        {
            DownloadNext(1);
        }


        /// <summary>
        /// Download <see cref="Video"/>'s parallel.
        /// </summary>
        /// <param name="parallelDownloads">Amount of parallel downloads</param>
        public void DownloadNext(int parallelDownloads)
        {
            ThreadPool.SetMaxThreads(parallelDownloads, 0);
            for (var i = 0; i < Count; i++)
            {
                ThreadPool.QueueUserWorkItem(Download, Dequeue());
            }
            
        }

        private async void Download(object downloadObj)
        {
            if (!(downloadObj is DownloadItem item)) return;
            var video = item.Video;

            //Get the video ID
            var id = YoutubeClient.ParseVideoId(video.GetUrl());
            var client = new YoutubeClient();


            //Get information from the youtube video
            var streamInfoSet = client.GetVideoMediaStreamInfosAsync(id).Result;


            //Get best audio stream
            var audioStream = streamInfoSet.Audio.WithHighestBitrate();

            //TODO Handle progress
            //Set the download progress to the progress of the item
            var progress = new Progress<double>(p => item.Progress = p);


            //Download to temp file
            var tempFileName = Path.GetTempFileName();
            using (var fileStream = File.Create(tempFileName, 1024))
            {
                client.DownloadMediaStreamAsync(audioStream, fileStream, progress).Wait();
            }

            Debug.WriteLine("Download complete!");
            var songPath = Path.Combine(MainWindow.MusicDir, $"{PathUtil.Sanitize(video.Title)}.mp3");


            //Convert the file from WebM (or whatever format is used) to mp3
            var file = new Ffmpeg.Converter().Engine.ConvertAsync(new MediaFile(tempFileName),
                new MediaFile(songPath)).Result;


            //Delete the temp file
            File.Delete(tempFileName);
            ProcessedSongs.Add(new Song(){AbsolutePath = file.FileInfo.FullName});


            //TODO change this to be more dynamic and robust

            //Get song details from youtube.
            var song = SongDetailParser.Get(item);

            //Add missing params
            song.Duration = video.Duration.ToString();
            song.AbsolutePath = file.FileInfo.FullName;

            //Write meta data to file
            var tagHandler = new TagHandler(song);
            tagHandler.SetTags();

            //Set the cover
            var coverParser = new CoverParser(song.Title, song.Artist);
            try
            {
                var bitmapCover = await coverParser.GetAsync();
                if (bitmapCover != null)
                {
                    tagHandler.SetCover(bitmapCover);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Debug.WriteLine("Conversion complete!");
        }
    }
}