using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FFmpeg.NET;
using Robeats_Desktop.Ffmpeg;
using Robeats_Desktop.Gui.Music;
using Robeats_Desktop.Scrape;
using Robeats_Desktop.UserControls;
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
        public ObservableCollection<DownloadControl> DownloadControls { get; set; }

        public static readonly string OutputDir = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        private readonly Converter _converter;

        private bool _isProgressIndeterminate;

        private double _progress;

        public MainWindow()
        {
            InitializeComponent();
            DownloadControls = new ObservableCollection<DownloadControl>();
            _converter = new Converter(new Engine(@"ffmpeg.exe"));
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
            ProcessVideo();
        }


        /// <summary>
        /// Get the web video async
        /// </summary>
        /// <param name="url"></param>
        /// <returns>return a <see cref="Task{TResult}"/></returns>
        private async Task<VideoInfo> GetVideoAsync(string url)
        {
            var id = YoutubeClient.ParseVideoId(url);
            var client = new YoutubeClient();
            return new VideoInfo {Url = url, Video = await client.GetVideoAsync(id), YoutubeClient = client};
        }


        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabItemMusic.IsSelected)
            {
                var musicItems = new List<MusicItem>();
                var files = Directory.GetFiles(OutputDir, "*.mp3")
                    .Select(Path.GetFileName).ToArray();
                foreach (var file in files)
                {
                    var tFile = TagLib.File.Create(Path.Combine(OutputDir, file));
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

        private void TextBoxUrl_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter)
            {
                ProcessVideo();
            }
        }

        private void ProcessVideo()
        {
            IsProgressIndeterminate = true;
            Task.Run(() =>
            {
                string url = null;
                TextBoxUrl.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    (ThreadStart) delegate { url = TextBoxUrl.Text; }
                );
                return GetVideoAsync(url);
            }).ContinueWith(videoInfo =>
            {
                var video = videoInfo.Result.Video;
                Application.Current.Dispatcher.Invoke((Action) delegate
                {
                    var downloadControl = new DownloadControl
                    {
                        Title = video.Title,
                        Uploader = video.Author,
                        Duration = video.Duration.ToString(),
                        Progress = 0,
                        Source = Thumbnail.Download(video.Thumbnails.MediumResUrl)
                    };
                    foreach (var control in DownloadControls)
                    {
                        if (control.Title.Equals(downloadControl.Title)) return;
                    }

                    DownloadControls.Add(downloadControl);
                    ListViewDownloads.ItemsSource = DownloadControls;
                    downloadControl.DownloadVideo(videoInfo.Result);
                });
            });
            IsProgressIndeterminate = false;
        }
    }
}