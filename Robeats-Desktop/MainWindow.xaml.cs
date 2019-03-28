using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AngleSharp;
using Robeats_Desktop.Util;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using Video = YoutubeExplode.Models.Video;

namespace Robeats_Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isProgressIndeterminate;

        public bool IsProgressIndeterminate
        {
            get => _isProgressIndeterminate;
            private set { ProgressBarStatus.IsIndeterminate = value;
                _isProgressIndeterminate = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GetVideoDetails();
            DownloadVideo();
        }

        private async void GetVideoDetails()
        {
            IsProgressIndeterminate = true;
            var url = TextBoxUrl.Text;
            var id = YoutubeClient.ParseVideoId(url);
            var client = new YoutubeClient();
            var video = await client.GetVideoAsync(id);

            LabelTitle.Content = video.Title;
            LabelAuthor.Content = video.Author;
            LabelDuration.Content = video.Duration;
            ImageThumbnail.Source = Thumbnail.Download(video.Thumbnails.MediumResUrl);
            IsProgressIndeterminate = false;
        }

        private async void DownloadVideo()
        {
            var client = new YoutubeClient();
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync("bnsUkE8i0tU");

            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
            var ext = streamInfo.Container.GetFileExtension();
            await client.DownloadMediaStreamAsync(streamInfo, $"downloaded_video.{ext}");
        }
    }
}
