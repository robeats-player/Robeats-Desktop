using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.Win32;
using Robeats_Desktop.DataTypes;
using MediaPlayer = Windows.Media.Playback.MediaPlayer;

namespace Robeats_Desktop.Player
{
    class AudioPlayer
    {
        public MediaPlayer MediaPlayer { get;}    
        private Slider Slider { get; set; }
        private TextBlock TextBlockProgress { get; set; }
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        public SystemMediaTransportControls SystemControls { get; set; }

        public AudioPlayer(Slider trackSlider, TextBlock textBlockProgress)
        {
            Slider = trackSlider;
            MediaPlayer = new MediaPlayer();
            DispatcherTimer timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            timer.Tick += timer_Tick;
            timer.Start();
            SystemControls = MediaPlayer.SystemMediaTransportControls;

            TextBlockProgress = textBlockProgress;

            SystemControls.ButtonPressed += SystemControls_ButtonPressed;
            SystemControls.IsPlayEnabled = true;
            SystemControls.IsPauseEnabled = true;
            SystemControls.IsNextEnabled = true;
            SystemControls.IsPreviousEnabled = true;
        }

        private void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((MediaPlayer.Source != null) && (!userIsDraggingSlider))
            {
                Slider.Minimum = 0;
                Slider.Maximum = MediaPlayer.PlaybackSession.NaturalDuration.TotalSeconds;
                Slider.Value = MediaPlayer.PlaybackSession.Position.TotalSeconds;
            }
        }


        public void Play()
        {
            MediaPlayer.Play();
            mediaPlayerIsPlaying = true;
        }

        public void Play(Song nextSong)
        {
            MediaPlayer.Source = MediaSource.CreateFromUri(new Uri(nextSong.AbsolutePath));
            Play();
        }

        public void PlayNext()
        {
            //TODO implement
        }

        public void Pause()
        {
            MediaPlayer.Pause();
            mediaPlayerIsPlaying = false;
        }


        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(Slider.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TextBlockProgress.Text = TimeSpan.FromSeconds(Slider.Value).ToString(@"hh\:mm\:ss");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MediaPlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        public void SetThumb(Song song)
        {
            
            //SystemControls.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromFile(StorageFile.GetFileFromPathAsync(song.AbsolutePath).GetResults());
        }

        public void SetSongInfo(Song song)
        {
            var updater = SystemControls.DisplayUpdater;
            updater.Type = MediaPlaybackType.Music;
            if (song.Artist != null)
                updater.MusicProperties.Artist = song.Artist;
            if (song.Album != null)
                updater.MusicProperties.AlbumTitle = song.Album;
            if (song.Title != null)
                updater.MusicProperties.Title = song.Title;

            updater.Update();
        }
    }
}
