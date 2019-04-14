using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Robeats_Desktop.DataTypes
{
    public class ImageHelper
    {
        public static BitmapImage GetImage(string url)
        {
            return GetImage(new Uri(url));
        }

        public static BitmapImage GetImage(Uri url)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = url;
            bitmap.EndInit();
            return bitmap;
        }

        public static Stream GetAsStream(string imageUrl)
        {

            using (var client = new WebClient())
            {
                return client.OpenRead(imageUrl);
            }
        }

        public static Bitmap GetAsBitmap(string imageUrl)
        {
            using (var ms = GetAsStream(imageUrl))
            {
                if (ms == null) return null;
                var bitmap = new Bitmap(ms);
                return bitmap;
            }
        }

    }
}
