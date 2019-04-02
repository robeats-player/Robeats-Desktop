using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FFmpeg.NET;
using Robeats_Desktop.Ffmpeg;
using Robeats_Desktop.Gui.Music;
using Robeats_Desktop.Scrape;
using Robeats_Desktop.Util;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using Path = System.IO.Path;
using UtilPath = Robeats_Desktop.Util.Path;

namespace Robeats_Desktop
{
    public partial class MainWindow : Window
    {
        public static readonly string OUTPUT_DIR = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        private readonly Converter _converter;

        private bool _isProgressIndeterminate;

        private double _progress;

        public MainWindow()
        {
            InitializeComponent();
            _converter = new Converter(new Engine(@"ffmpeg.exe"));
            SongMeta = new SongMetaData();
            
        }

        public SongMetaData SongMeta { get; set; }
        public string Id { get; set; }
        public YoutubeClient Client { get; set; }

        public double Progress
        {
            get => _progress;
            private set
            {
                _progress = value;
                Application.Current.Dispatcher.Invoke(() => { ProgressBarDownload.Value = _progress; });
            }
        }

        public bool IsProgressIndeterminate
        {
            get => _isProgressIndeterminate;
            private set
            {
                ProgressBarStatus.IsIndeterminate = value;
                _isProgressIndeterminate = value;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsProgressIndeterminate = true;
            Task.Run(() =>
            {
                string url = null;
                TextBoxUrl.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    (ThreadStart) delegate { url = TextBoxUrl.Text; }
                );
                return GetVideoDetails(url);
            }).ContinueWith(songInfo =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GridInfo.Visibility = Visibility.Visible;
                    IsProgressIndeterminate = false;
                    LabelTitle.Content = songInfo.Result.Video.Title;
                    LabelAuthor.Content = songInfo.Result.Video.Author;
                    LabelDuration.Content = songInfo.Result.Video.Duration;
                    ImageThumbnail.Source = Thumbnail.Download(songInfo.Result.Video.Thumbnails.MediumResUrl);
                });

                DownloadVideo(songInfo.Result);
            });
        }

        private async Task<SongInfo> GetVideoDetails(string url)
        {
            Id = YoutubeClient.ParseVideoId(url);
            Client = new YoutubeClient();
            var songInfo = new SongInfo();
            songInfo.Url = url;
            songInfo.Video = await Client.GetVideoAsync(Id);
            return songInfo;
        }

        private async void DownloadVideo(SongInfo songInfo)
        {
            var video = songInfo.Video;
            var title = UtilPath.Sanitize(video.Title);

            //Get all information from the youtube video
            var streamInfoSet = await new YoutubeClient().GetVideoMediaStreamInfosAsync(Id);

            //Pick the audio file with the best quality
            var streamInfo = streamInfoSet.Audio.WithHighestBitrate();

            // Set up progress handler
            var progressHandler = new Progress<double>(p => Progress = p);

            // Download to a temporary file
            var tempFileName = Path.GetTempFileName();
            using (var fileStream = File.Create(tempFileName, 1024))
            {
                await Client.DownloadMediaStreamAsync(streamInfo, fileStream, progressHandler);
            }

            //Do the conversion from WebM (or whatever format YouTube might use in the future)
            var file = await _converter.Engine.ConvertAsync(new MediaFile(tempFileName),
                new MediaFile(Path.Combine(OUTPUT_DIR, $"{title}.mp3")));


            //Delete the temporary file
            File.Delete(tempFileName);

            //Write meta data to song
            SongMeta.Path = file.FileInfo.FullName;
            songInfo.SetTags(SongMeta.TagFile);
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabItemMusic.IsSelected)
            {
                var musicItems = new List<MusicItem>();
                var files = Directory.GetFiles(OUTPUT_DIR, "*.mp3")
                    .Select(Path.GetFileName).ToArray();
                foreach (var file in files)
                {
                    var tFile = TagLib.File.Create(Path.Combine(OUTPUT_DIR, file));
                    var title = tFile.Tag.Title ?? Path.GetFileNameWithoutExtension(file);
                    var musicItem = new MusicItem(title, tFile.Tag.FirstPerformer,
                        $"{tFile.Properties.Duration.Minutes}:{tFile.Properties.Duration.Seconds:D2}");
                    musicItems.Add(musicItem);
                }

                ListViewSongs.ItemsSource = musicItems;
            }
        }

        private void TextBoxUrl_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBoxUrl.SelectionStart = 0;
            TextBoxUrl.SelectionLength = TextBoxUrl.Text.Length;
        }
    }
}