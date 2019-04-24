using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.Xaml.Media;

namespace Robeats_Desktop.Player
{
    class UniversalPlayer
    {

        public MediaElement MediaElement { get; set; }

        public UniversalPlayer(MediaElement mediaElement)
        {
            var systemControls = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            //systemControls.ButtonPressed += SystemControls_ButtonPressed;
            //mediaElement. += MediaElement_CurrentStateChanged;
            systemControls.IsPlayEnabled = true;
            systemControls.IsPauseEnabled = true;
            MediaElement = mediaElement;
        }

        



        /*void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    PlayMedia();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    PauseMedia();
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    StopMedia();
                    break;
                default:
                    break;
            }
        }*/

        private void StopMedia()
        {
            Task.Run(() =>
            {
                MediaElement.Stop();
            });
        }

        /*async void PlayMedia()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (MediaElement. == MediaElementState.Playing)
                    mediaElement.Pause();
                else
                    mediaElement.Play();
            });
        }*/

        /*async void PauseMedia()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                mediaElement.Pause();
            });
        }*/
    }
}
