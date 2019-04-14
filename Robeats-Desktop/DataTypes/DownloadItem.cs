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
using Robeats_Desktop.DataTypes;
using YoutubeExplode.Models;

namespace Robeats_Desktop.DataTypes
{
    public class DownloadItem : INotifyPropertyChanged
    {
        public Video Video { get; set; }
        public ImageSource Source { get; set; }
        private double _progress;
        private double _oldProgress;
        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                if (_progress > _oldProgress + 0.02)
                {
                    _oldProgress = _progress;
                    OnPropertyChanged();
                }
            }
        }
        public DownloadItem(Video video)
        {
            Progress = 0;
            _oldProgress = 0;
            Video = video;
            Source = ImageHelper.GetImage(Video.Thumbnails.MediumResUrl);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}