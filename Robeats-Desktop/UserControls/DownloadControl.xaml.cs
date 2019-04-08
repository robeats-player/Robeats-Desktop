using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AngleSharp;
using FFmpeg.NET;
using Robeats_Desktop.Annotations;
using Robeats_Desktop.Ffmpeg;
using Robeats_Desktop.Util;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using Path = System.IO.Path;
using FFmpeg.NET.Events;
using Robeats_Desktop.DataTypes;

namespace Robeats_Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for DownloadControl.xaml
    /// </summary>
    public partial class DownloadControl : UserControl, INotifyPropertyChanged
    {
        public DownloadControl()
        {
            InitializeComponent();
        }

        /*private void OnProcessComplete(object sender, ConversionCompleteEventArgs e)
        {
            IsProgressIndeterminate = false;
        }*/


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = MouseWheelEvent, Source = sender
                };
                var parent = ((Control)sender).Parent as UIElement;
                parent?.RaiseEvent(eventArg);
            }
        }
    }
}