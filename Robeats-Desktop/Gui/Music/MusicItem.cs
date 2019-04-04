using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace Robeats_Desktop.Gui.Music
{
    class MusicItem
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Duration { get; set; }
        public ContextMenu ContextMenuItem { get; set; }

        public MusicItem(string title, string artist, string duration)
        {
            Title = title;
            Artist = artist;
            Duration = duration;

            MenuItem mi = new MenuItem();
            ContextMenuItem = new ContextMenu();
            mi.Header = "NO U";
            ContextMenuItem.Items.Add(mi);
        }
    }
}