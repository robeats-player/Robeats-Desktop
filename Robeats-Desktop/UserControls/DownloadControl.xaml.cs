using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AngleSharp;
using FFmpeg.NET;
using Robeats_Desktop.Annotations;
using Robeats_Desktop.Ffmpeg;
using Robeats_Desktop.Scrape;
using Robeats_Desktop.Util;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using UtilPath = Robeats_Desktop.Util.Path;
using Path = System.IO.Path;

namespace Robeats_Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for DownloadControl.xaml
    /// </summary>
    public partial class DownloadControl : UserControl, INotifyPropertyChanged
    {
        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(string));


        public string Uploader
        {
            get => (string) GetValue(UploaderProperty);
            set => SetValue(UploaderProperty, value);
        }

        // Using a DependencyProperty as the backing store for Uploader.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UploaderProperty =
            DependencyProperty.Register("Uploader", typeof(string), typeof(DownloadControl));


        public double Progress
        {
            get => (double) GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        // Using a DependencyProperty as the backing store for Progress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(DownloadControl));



        public bool IsProgressIndeterminate
        {
            get => (bool)GetValue(IsProgressIndeterminateProperty);
            set => SetValue(IsProgressIndeterminateProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsProgressIndeterminate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsProgressIndeterminateProperty =
            DependencyProperty.Register("IsProgressIndeterminate", typeof(bool), typeof(DownloadControl), new PropertyMetadata(false));



        public string Duration
        {
            get => (string) GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        // Using a DependencyProperty as the backing store for Duration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(string), typeof(DownloadControl));


        public BitmapImage Source
        {
            get => (BitmapImage) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for BitmapImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(BitmapImage), typeof(DownloadControl));


        private readonly Converter _converter;
        private readonly MainWindow _main;

        public DownloadControl()
        {
            InitializeComponent();
            _main = (MainWindow) Application.Current.MainWindow;
            _converter = new Converter(new Engine("ffmpeg.exe"));
        }


        public async void DownloadVideo(VideoInfo videoInfo)
        {
            var video = videoInfo.Video;
            var title = UtilPath.Sanitize(video.Title);

            //Get all information from the youtube video
            var streamInfoSet = await new YoutubeClient().GetVideoMediaStreamInfosAsync(video.Id);

            //Pick the audio file with the best quality
            var streamInfo = streamInfoSet.Audio.WithHighestBitrate();

            // Set up progress handler
            var progressHandler = new Progress<double>(p => Progress = p);

            // Download to a temporary file
            var tempFileName = Path.GetTempFileName();
            using (var fileStream = File.Create(tempFileName, 1024))
            {
                await videoInfo.YoutubeClient.DownloadMediaStreamAsync(streamInfo, fileStream, progressHandler);
            }

            //Do the conversion from WebM (or whatever format YouTube might use in the future)
            var file = await _converter.Engine.ConvertAsync(new MediaFile(tempFileName),
                 new MediaFile(Path.Combine(MainWindow.OutputDir, $"{title}.mp3")));


            //Delete the temporary file
            File.Delete(tempFileName);

            //Write meta data to song
            var tagWrapper = new TagWrapper {Path = file.FileInfo.FullName};
            videoInfo.SetTags(tagWrapper.TagFile);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}