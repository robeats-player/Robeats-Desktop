using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FFmpeg.NET;
using FFmpeg.NET.Events;
using Robeats_Desktop.Annotations;
using Robeats_Desktop.DataTypes;
using Robeats_Desktop.Ffmpeg;
using Robeats_Desktop.Network;
using Robeats_Desktop.Network.Frames;
using Robeats_Desktop.UserControls;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using Path = System.IO.Path;
using Playlist = YoutubeExplode.Models.Playlist;

namespace Robeats_Desktop
{
    public partial class MainWindow : Window
    {
        public static readonly string OutputDir = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        public ObservableCollection<Song> Songs
        {
            get => (ObservableCollection<Song>)GetValue(SongsProperty);
            set => SetValue(SongsProperty, value);
        }

        // Using a DependencyProperty as the backing store for Songs.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SongsProperty =
            DependencyProperty.Register("Songs", typeof(ObservableCollection<Song>), typeof(MainWindow));


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Downloads = new ObservableCollection<DownloadItem>();
            Songs = new ObservableCollection<Song>();
        }

        public ObservableCollection<DownloadItem> Downloads
        {
            get => (ObservableCollection<DownloadItem>) GetValue(DownloadsProperty);
            set => SetValue(DownloadsProperty, value);
        }

        // Using a DependencyProperty as the backing store for Downloads.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DownloadsProperty =
            DependencyProperty.Register("Downloads", typeof(ObservableCollection<DownloadItem>), typeof(MainWindow));


        public bool IsProgressIndeterminate
        {
            get => (bool) GetValue(IsProgressIndeterminateProperty);
            set => SetValue(IsProgressIndeterminateProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsProgressIndeterminate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsProgressIndeterminateProperty =
            DependencyProperty.Register("IsProgressIndeterminate", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));


        private void ButtonDownload_Click(object sender, RoutedEventArgs e)
        {
            //Check if playlist checkbox is checked, if so download the playlist instead of the song.
            if (CheckBoxPlaylist.IsChecked != null && CheckBoxPlaylist.IsChecked.Value)
            {
                ProcessPlaylist(TextBoxUrl.Text);
            }
            else
            {
                var url = TextBoxUrl.Text;
                Task.Run(() =>
                {
                    Task.WaitAll(ProcessVideo(url));
                    DownloadQueue.DownloadNext();
                });
            }
        }


        /// <summary>
        /// Get the web video async
        /// </summary>
        /// <param name="url"></param>
        /// <returns>return a <see cref="Task{TResult}"/></returns>
        private async Task<Video> GetVideoAsync(string url)
        {
            var id = YoutubeClient.ParseVideoId(url);
            var client = new YoutubeClient();
            return await client.GetVideoAsync(id);
        }

        private async Task<Playlist> GetPlaylistAsync(string url)
        {
            try
            {
                var id = YoutubeClient.ParsePlaylistId(url);
                var client = new YoutubeClient();
                return await client.GetPlaylistAsync(id);
            }
            catch (FormatException ex)
            {
                Debug.WriteLine(ex.StackTrace);
                return null;
            }
            
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabItemMusic.IsSelected)
            {
                if (Songs.Count == 0)
                {
                    GetLocalSongsAsync();
                }
            }
        }

        private async void GetLocalSongsAsync()
        {
            await Task.Run(() =>
            {
                var files = Directory.GetFiles(OutputDir, "*.mp3")
                    .Select(Path.GetFileName).ToArray();
                foreach (var file in files)
                {
                    try
                    {
                        var fullName = Path.Combine(OutputDir, file);
                        var tFile = TagLib.File.Create(fullName);
                        var title = tFile.Tag.Title ?? Path.GetFileNameWithoutExtension(file);
                        var musicItem = new Song(title, tFile.Tag.FirstPerformer,
                            $"{(int)tFile.Properties.Duration.TotalMinutes}:{tFile.Properties.Duration.Seconds:D2}",
                            Md_5.Calculate(fullName));
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            Songs.Add(musicItem);
                        });

                    }
                    catch (Exception)
                    {
                        // ignored
                        // Thrown when an item is being used by another program or still being converted.
                    }
                }

            });
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
                if (CheckBoxPlaylist.IsChecked != null && CheckBoxPlaylist.IsChecked.Value)
                {
                    ProcessPlaylist(TextBoxUrl.Text);
                }
                else
                {
                    var url = TextBoxUrl.Text;
                    Task.Run(() =>
                    {
                        Task.WaitAll(ProcessVideo(url));
                        DownloadQueue.DownloadNext();
                    });
                }
            }
        }


        private void ProcessPlaylist(string url)
        {
            Task.Run(() => GetPlaylistAsync(url)).ContinueWith(playlistResult =>
            {
                if(playlistResult.Result == null) return;
                var playlist = playlistResult.Result;
                var tasks = new Task[playlist.Videos.Count];
                for (var i = 0; i < playlist.Videos.Count; i++)
                {
                    tasks[i] = ProcessVideo(playlist.Videos[i].GetUrl());
                }

                Task.WaitAll(tasks);

            }).ContinueWith(task =>
            {
                while (DownloadQueue.HasNext())
                {
                    //TODO make user configurable
                    if (DownloadQueue.Count() > 3)
                    {
                        DownloadQueue.DownloadNext(3);
                    }
                    else
                    {
                        DownloadQueue.DownloadNext(DownloadQueue.Count());
                    }
                }
            });
        }


        private async Task ProcessVideo(string url)
        {
            Application.Current.Dispatcher.Invoke(delegate { IsProgressIndeterminate = true; });
            var video = await GetVideoAsync(url);
            Application.Current.Dispatcher.Invoke(delegate
            {
                var download = new DownloadItem(video.Title, video.Author, video.Duration.ToString(), "",
                    video.Thumbnails.MediumResUrl, video.GetUrl());
                foreach (var control in Downloads)
                {
                    if (control.Title.Equals(download.Title))
                    {
                        IsProgressIndeterminate = false;
                        return;
                    }
                }

                IsProgressIndeterminate = false;
                Downloads.Add(download);
                DownloadQueue.Add(download);
                //DownloadVideo(videoInfo.Result);
            });
        }

        private static void IoMethod()
        {
            var discovery = new DeviceDiscovery();
            discovery.SendRequest(ProtocolRequest.DeviceDiscovery);
            var list = discovery.AwaitResponses(8000);
            foreach (var robeatsDevice in list)
            {
                Console.WriteLine($@"Request received from:{robeatsDevice.EndPoint}");
            }
        }

        private void ButtonFindDevices_Click(object sender, RoutedEventArgs e)
        {
            //IsProgressIndeterminate = true;
            Task.Run(() => { IoMethod(); });
        }

        private void ButtonSendInfo_Click(object sender, RoutedEventArgs e)
        {
            var discovery = new DeviceDiscovery();
            discovery.SendDiscoveryReply(new RobeatsDevice()
            {
                Name = "YEET",
                Id = 240
            });
        }
    }
}