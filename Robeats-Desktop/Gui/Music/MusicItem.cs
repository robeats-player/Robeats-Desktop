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


        public MusicItem(string title, string artist, string duration)
        {
            Title = title;
            Artist = artist;
            Duration = duration;
        }

        public MusicItem()
        {
        }

        public void Add(StackPanel stackPanel)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var title = new Label() {Content = Title};
            title.SetValue(Grid.ColumnProperty, 1);
            grid.Children.Add(title);

            var artist = new Label() { Content = Artist };
            artist.SetValue(Grid.ColumnProperty, 2);
            grid.Children.Add(artist);

            var duration = new Label() { Content = Duration };
            duration.SetValue(Grid.ColumnProperty, 3);
            grid.Children.Add(duration);

            var play = new PackIcon(){Kind = PackIconKind.PlayBoxOutline, Visibility = Visibility.Hidden};
            grid.MouseEnter += GridOnMouseEnter;
            grid.MouseLeave += GridOnMouseLeave;
            play.SetValue(Grid.ColumnProperty, 0);
            grid.Children.Add(play);
            stackPanel.Children.Add(grid);
        }

        public void Clear(StackPanel stackPanel)
        {
            stackPanel.Children.Clear();
        }
        private void GridOnMouseLeave(object sender, MouseEventArgs e)
        {
            var element = ((Grid)e.Source).Children.OfType<PackIcon>().FirstOrDefault();
            if (element != null) element.Visibility = Visibility.Hidden;
        }

        private void GridOnMouseEnter(object sender, MouseEventArgs e)
        {
            var element = ((Grid) e.Source).Children.OfType<PackIcon>().FirstOrDefault();
            if (element != null) element.Visibility = Visibility.Visible;
        }
    }
}