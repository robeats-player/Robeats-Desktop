using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
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
using Robeats_Desktop.Network;
using Robeats_Desktop.Network.Frames;

namespace Robeats_Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for NetworkControl.xaml
    /// </summary>
    public partial class NetworkControl : UserControl
    {

        public ObservableCollection<RobeatsDevice> RobeatsDevices
        {
            get => (ObservableCollection<RobeatsDevice>)GetValue(RobeatsDevicesProperty);
            set => SetValue(RobeatsDevicesProperty, value);
        }

        // Using a DependencyProperty as the backing store for RobeatsDevices.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RobeatsDevicesProperty =
            DependencyProperty.Register("RobeatsDevices", typeof(ObservableCollection<RobeatsDevice>), typeof(NetworkControl));


        public NetworkControl()
        {
            RobeatsDevices = new ObservableCollection<RobeatsDevice>();
            InitializeComponent();
        }
    }
}
