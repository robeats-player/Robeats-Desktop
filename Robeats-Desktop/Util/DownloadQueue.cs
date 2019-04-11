using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.NET;
using Robeats_Desktop.DataTypes;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace Robeats_Desktop.Util
{
    public static class DownloadQueue
    {
        private static readonly Queue<DownloadItem> Queue;


        static DownloadQueue()
        {
            Queue = new Queue<DownloadItem>();
        }

        public static void Add(DownloadItem item)
        {
            Queue.Enqueue(item);
        }

        public static int Count()
        {
            return Queue.Count;
        }

        public static bool HasNext()
        {
            return Queue.Count > 0;
        }

        public static void DownloadNext()
        {
            DownloadNext(1);
        }

        public static void DownloadNext(int parallelDownloads)
        {
            var tasks = new Task[parallelDownloads];
            for (var i = 0; i < parallelDownloads; i++)
            {
                var item = Queue.Dequeue();
                tasks[i] = Download(item);
            }

            Task.WaitAll(tasks);
        }

        private static async Task Download(DownloadItem item)
        {
            //Get the video ID
            var id = YoutubeClient.ParseVideoId(item.DownloadUrl);
            var client = new YoutubeClient();
            //Get information from the youtube video
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);

            //Get best audio stream
            var audioStream = streamInfoSet.Audio.WithHighestBitrate();

            //Set the download progress to the progress of the item
            var progress = new Progress<double>(p => item.Progress = p);

            //Download to temp file
            var tempFileName = Path.GetTempFileName();
            using (var fileStream = File.Create(tempFileName, 1024))
            {
                await client.DownloadMediaStreamAsync(audioStream, fileStream, progress);
            }

            Debug.WriteLine("Download complete!");

            //Convert the file from WebM (or whatever format is used)
            var file = await new Ffmpeg.Converter().Engine.ConvertAsync(new MediaFile(tempFileName),
                new MediaFile(Path.Combine(MainWindow.OutputDir, $"{PathUtil.Sanitize(item.Title)}.mp3")));

            //Delete the temp file
            File.Delete(tempFileName);

            //Write meta data
            var tagWrapper = new TagWrapper(file.FileInfo.FullName, item.DownloadUrl);
            tagWrapper.SetTags();
            tagWrapper.SetCover();
            Debug.WriteLine("Conversion complete!");
        }
    }
}