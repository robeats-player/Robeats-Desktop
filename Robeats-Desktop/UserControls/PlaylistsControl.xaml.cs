using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using MaterialDesignThemes.Wpf;
using Robeats_Desktop.DataTypes;

namespace Robeats_Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for PlaylistsControl.xaml
    /// </summary>
    public partial class PlaylistsControl : UserControl
    {


        public ObservableCollection<Playlist> Playlists
        {
            get => (ObservableCollection<Playlist>)GetValue(PlaylistsProperty);
            set => SetValue(PlaylistsProperty, value);
        }

        // Using a DependencyProperty as the backing store for Playlists.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaylistsProperty =
            DependencyProperty.Register("Playlists", typeof(ObservableCollection<Playlist>), typeof(PlaylistsControl));


        public PlaylistsControl()
        {
            Playlists = new ObservableCollection<Playlist>();
            var songs = new HashSet<Song> { new Song("Yeet", "no U", "5:32", "-") };
            Playlists.Add(new Playlist());
            
            InitializeComponent();
            
        }
    }
}
