using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AngleSharp;
using FFmpeg.NET;
using Robeats_Desktop.Ffmpeg;
using Robeats_Desktop.Gui.Music;
using Robeats_Desktop.Util;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using Path = System.IO.Path;
using UtilPath = Robeats_Desktop.Util.Path;
using Video = YoutubeExplode.Models.Video;

namespace Robeats_Desktop
{
    public partial class MainWindow : Window
    {
        public string Url { get; set; }
        public string Id { get; set; }
        public YoutubeClient Client { get; set; }
        public static readonly string OUTPUT_DIR = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        private Converter _converter;

        private double _progress;

        public double Progress
        {
            get => _progress;
            private set
            {
                _progress = value;
                Application.Current.Dispatcher.Invoke(() => { ProgressBarDownload.Value = _progress; });
            }
        }

        private bool _isProgressIndeterminate;

        public bool IsProgressIndeterminate
        {
            get => _isProgressIndeterminate;
            private set
            {
                ProgressBarStatus.IsIndeterminate = value;
                _isProgressIndeterminate = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            _converter = new Converter(new Engine(@"ffmpeg.exe"));
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
            }).ContinueWith((video) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GridInfo.Visibility = Visibility.Visible;
                    IsProgressIndeterminate = false;
                    LabelTitle.Content = video.Result.Title;
                    LabelAuthor.Content = video.Result.Author;
                    LabelDuration.Content = video.Result.Duration;
                    ImageThumbnail.Source = Thumbnail.Download(video.Result.Thumbnails.MediumResUrl);
                });
                DownloadVideo(video.Result);
            });
        }

        private async Task<Video> GetVideoDetails(string url)
        {
            Id = YoutubeClient.ParseVideoId(url);
            Client = new YoutubeClient();
            return await Client.GetVideoAsync(Id);
        }

        private async void DownloadVideo(Video video)
        {
            var streamInfoSet = await new YoutubeClient().GetVideoMediaStreamInfosAsync(Id);

            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
            var ext = streamInfo.Container.GetFileExtension();


            // Set up progress handler
            var progressHandler = new Progress<double>(p => Progress = p);

            // Download to memory to then convert to audio only formats
            var memoryStream = new MemoryStream();
            await Client.DownloadMediaStreamAsync(streamInfo,
                memoryStream, progressHandler);
            var tempFileName = Path.GetTempFileName();
            var fileStream = File.Create(tempFileName, memoryStream.Capacity, FileOptions.DeleteOnClose);
            memoryStream.WriteTo(fileStream);
            //var mediaFile = await _converter.Convert(new MediaFile(tempFileName), OUTPUT_DIR, UtilPath.Sanitize(video.Title));
            MediaFile file = await _converter.Engine.ConvertAsync(new MediaFile(tempFileName),
                 new MediaFile(Path.Combine(OUTPUT_DIR, $"{UtilPath.Sanitize(video.Title)}.mp3")));
            Debug.WriteLine(file.FileInfo);
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabItemMusic.IsSelected)
            {
                var musicItem = new MusicItem();
                musicItem.Clear(StackPanelSongs);
                var files = Directory.GetFiles(OUTPUT_DIR, "*.mp3")
                    .Select(Path.GetFileName).ToArray();
                foreach (var file in files)
                {
                    var tFile = TagLib.File.Create(Path.Combine(OUTPUT_DIR, file));
                    musicItem = new MusicItem(Path.GetFileNameWithoutExtension(file), tFile.Tag.FirstAlbumArtist,
                        $"{(int) tFile.Properties.Duration.TotalMinutes}:{tFile.Properties.Duration.Seconds}");
                    musicItem.Add(StackPanelSongs);
                }
            }
        }

        private void TextBoxUrl_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBoxUrl.SelectionStart = 0;
            TextBoxUrl.SelectionLength = TextBoxUrl.Text.Length;
        }
    }
}