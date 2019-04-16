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
using System.Windows.Documents;
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
using Robeats_Desktop.Util;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using Path = System.IO.Path;
using Playlist = YoutubeExplode.Models.Playlist;

namespace Robeats_Desktop
{
    public partial class MainWindow : Window
    {
        public static readonly string MusicDir = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

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
            ProcessVideos();
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
                var files = Directory.GetFiles(MusicDir, "*.mp3")
                    .Select(Path.GetFileName).ToArray();
                foreach (var file in files)
                {
                    try
                    {
                        var fullName = Path.Combine(MusicDir, file);
                        var tFile = TagLib.File.Create(fullName);
                        var title = tFile.Tag.Title ?? Path.GetFileNameWithoutExtension(file);
                        var musicItem = new Song(title, tFile.Tag.FirstPerformer,
                            $"{(int)tFile.Properties.Duration.TotalMinutes}:{tFile.Properties.Duration.Seconds:D2}",
                             fullName);
                        musicItem.Hash = BitConverter.ToString(Md5.Calculate(fullName));
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
                ProcessVideos();
            }
        }

        private void ProcessVideos()
        {
            var url = TextBoxUrl.Text;

            //Check if playlist checkbox is checked, if so download the playlist instead of the song.
            if (CheckBoxPlaylist.IsChecked != null && CheckBoxPlaylist.IsChecked.Value)
            {
                //TODO make configurable

                Task.Run(() => Download.GetPlaylistAsync(url)).ContinueWith(playlistResult =>
                {
                    var downloadQueue = new DownloadQueue();
                    Application.Current.Dispatcher.Invoke(() =>
                    {

                        foreach (var resultVideo in playlistResult.Result.Videos)
                        {
                            var downloadItem = new DownloadItem(resultVideo);
                            Downloads.Add(downloadItem);
                            downloadQueue.Add(downloadItem);
                        }

                    });
                    Download.DownloadPlaylist(downloadQueue, 3);
                });
            }
            else
            {
                Task.Run(() => Download.GetVideoAsync(url)).ContinueWith(videoResult =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var downloadItem = new DownloadItem(videoResult.Result);
                        Downloads.Add(downloadItem);
                        Download.DownloadSong(new DownloadQueue { downloadItem });
                    });
                });
            }
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

                //TODO implement id's
                Name = TextBoxDeviceName.Text,
                Id = 240
            });
        }

        private void ButtonSync_Click(object sender, RoutedEventArgs e)
        {
            List<byte[]> bytes = new List<byte[]>();
            var songFiles = Directory.GetFiles(MusicDir, "*.mp3");
            foreach (var songFile in songFiles)
            {
                bytes.Add(Md5.Calculate(songFile));
            }
            //var songSender = new SongSyncSender();
            //var sync = new SongSyncListener();
            //sync.Listen();
        }
    }
}