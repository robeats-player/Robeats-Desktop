using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Robeats_Desktop.Annotations;
using Robeats_Desktop.Util;

namespace Robeats_Desktop.DataTypes
{
    public class DownloadItem : Song, INotifyPropertyChanged
    {
        public string ThumbnailUrl { get; set; }
        public ImageSource Source { get; set; }
        private double _progress;
        private double _oldProgress;
        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                if (_progress > _oldProgress + 0.1)
                {
                    _oldProgress = _progress;
                    OnPropertyChanged();
                }
            }
        }

        public string DownloadUrl { get; set; }

        public DownloadItem(string title, string artist, string duration, string hash, string thumbnailUrl, string downloadUrl) : base(title, artist,
            duration, hash)
        {
            Title = title;
            Artist = artist;
            Duration = duration;
            Hash = hash;
            ThumbnailUrl = thumbnailUrl;
            DownloadUrl = downloadUrl;
            Progress = 0;
            _oldProgress = 0;
            Source = Thumbnail.Download(thumbnailUrl);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}