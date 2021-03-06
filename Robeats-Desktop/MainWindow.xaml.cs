﻿using System;
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
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using FFmpeg.NET;
using FFmpeg.NET.Events;
using MaterialDesignThemes.Wpf;
using Robeats_Desktop.Annotations;
using Robeats_Desktop.DataTypes;
using Robeats_Desktop.Event;
using Robeats_Desktop.Ffmpeg;
using Robeats_Desktop.Network;
using Robeats_Desktop.Network.Frames;
using Robeats_Desktop.Player;
using Robeats_Desktop.UserControls;
using Robeats_Desktop.Util;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using MediaPlayer = Windows.Media.Playback.MediaPlayer;
using Path = System.IO.Path;
using Playlist = YoutubeExplode.Models.Playlist;

namespace Robeats_Desktop
{
    public partial class MainWindow : Window
    {
        private readonly AudioPlayer audioPlayer;

        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;

        public static readonly string MusicDir = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        public Thread MulticastListener;

        public ObservableCollection<Song> Songs
        {
            get => (ObservableCollection<Song>) GetValue(SongsProperty);
            set => SetValue(SongsProperty, value);
        }

        // Using a DependencyProperty as the backing store for Songs.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SongsProperty =
            DependencyProperty.Register("Songs", typeof(ObservableCollection<Song>), typeof(MainWindow));


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            MulticastListener = new Thread(IoMethod);
            MulticastListener.Start();
            Downloads = new ObservableCollection<DownloadItem>();
            Songs = new ObservableCollection<Song>();

            var timer = new DispatcherTimer(DispatcherPriority.Render) {Interval = TimeSpan.FromSeconds(1)};
            timer.Tick += timer_Tick;
            timer.Start();

            audioPlayer = new AudioPlayer(SliderProgress, LabelProgress);
            audioPlayer.MediaPlayer.AudioDeviceType = MediaPlayerAudioDeviceType.Multimedia;
            var systemControls =
                audioPlayer.MediaPlayer.SystemMediaTransportControls;

            systemControls.ButtonPressed += SystemControls_ButtonPressed;
            systemControls.IsPlayEnabled = true;
            systemControls.IsPauseEnabled = true;
            systemControls.IsNextEnabled = true;
            systemControls.IsPreviousEnabled = true;
        }

        void SystemControls_ButtonPressed(SystemMediaTransportControls sender,
            SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                        audioPlayer.Play();
                        break;
                    case SystemMediaTransportControlsButton.Pause:
                        audioPlayer.Pause();
                        break;
                    case SystemMediaTransportControlsButton.Stop:
                        audioPlayer.Pause();
                        break;
                    default:
                        break;
                }
            });
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((audioPlayer.MediaPlayer.Source != null) && (!userIsDraggingSlider))
            {
                SliderProgress.Minimum = 0;
                SliderProgress.Maximum = audioPlayer.MediaPlayer.PlaybackSession.NaturalDuration.TotalSeconds;
                SliderProgress.Value = audioPlayer.MediaPlayer.PlaybackSession.Position.TotalSeconds;
            }
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
                            $"{(int) tFile.Properties.Duration.TotalMinutes}:{tFile.Properties.Duration.Seconds:D2}",
                            fullName);
                        musicItem.Hash = BitConverter.ToString(Md5.Calculate(fullName));
                        Application.Current.Dispatcher.Invoke(delegate { Songs.Add(musicItem); });
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
                        Download.DownloadSong(new DownloadQueue {downloadItem});
                    });
                });
            }
        }

        private void IoMethod()
        {
            var discovery = new DeviceDiscovery();
            discovery.DeviceDetect += delegate(object sender, DeviceDiscoveryEventArgs args)
            {
                Console.WriteLine(args.RobeatsDevice.ToString());
                discovery.SendDiscoveryReply(args.RobeatsDevice);
            };
            discovery.DeviceDiscoverReply += delegate(object sender, DeviceDiscoveryEventArgs args)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var device in UserControlNetwork.RobeatsDevices)
                    {
                        if (device.Id == args.RobeatsDevice.Id) return;
                    }

                    UserControlNetwork.RobeatsDevices.Add(args.RobeatsDevice);
                    Debug.WriteLine($"Count:{UserControlNetwork.RobeatsDevices.Count}");
                });
            };

            discovery.AwaitMulticastRequest();
            Task.Run(() => { discovery.AwaitDiscoveryReply(); });
        }

        private void ButtonFindDevices_Click(object sender, RoutedEventArgs e)
        {
            //IsProgressIndeterminate = true;
            var name = TextBoxDeviceName.Text;
            if (byte.TryParse(TextBoxDeviceId.Text, out var id))
            {
                Task.Run(() =>
                {
                    var discovery = new DeviceDiscovery();
                    discovery.SendRequest(ProtocolRequest.DeviceDiscovery, new RobeatsDevice(name, id));
                    discovery.AwaitDiscoveryReply();
                });
            }
            else
            {
                Debug.WriteLine("Not a valid id");
                //TODO show error message
            }
        }

        private void ButtonSendInfo_Click(object sender, RoutedEventArgs e)
        {
            var discovery = new DeviceDiscovery();
            discovery.SendDiscoveryReply(new RobeatsDevice
            {
                //TODO implement id's
                Name = TextBoxDeviceName.Text,
                Id = byte.Parse(TextBoxDeviceId.Text)
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

            try
            {
                var songSender = new SongSyncSender(new IPEndPoint(IPAddress.Parse("192.168.1.8"), 4567));
                songSender.Sync(bytes);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }

            //var sync = new SongSyncListener();
            //sync.Listen();
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (audioPlayer != null) && (audioPlayer.MediaPlayer.Source != null);
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (((ListViewItem) sender).Content is Song song)
            {
                audioPlayer.Play(song);
                //LabelProgressTotalDuration.Content = MusicPlayer.NaturalDuration.TimeSpan.ToString(@"m\:ss");
                audioPlayer.SetSongInfo(song);
                audioPlayer.SetThumb(song);
                audioPlayer.Play();
                PackIconPlay.Kind = PackIconKind.PauseCircleFilled;
            }
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            audioPlayer.Pause();
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            audioPlayer.MediaPlayer.Dispose();
            mediaPlayerIsPlaying = false;
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            audioPlayer.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(SliderProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LabelProgress.Text = TimeSpan.FromSeconds(SliderProgress.Value).ToString(@"m\:ss");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            audioPlayer.MediaPlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {


            if (sender is ListViewItem)
            {
                Play_Executed(sender, null);
            }

            Debug.WriteLine("Clicked!");
        }
    }
}