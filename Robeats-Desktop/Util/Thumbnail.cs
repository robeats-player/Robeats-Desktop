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

namespace Robeats_Desktop.Util
{
    class Thumbnail
    {
        public static BitmapImage GetImage(string url)
        {
            return GetImage(new Uri(url));
        }

        public static BitmapImage GetImage(Uri url)
        {
            HttpClient client = new HttpClient();
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = url;
            bitmap.EndInit();
            return bitmap;
        }

        public static Bitmap DownloadImage(string imageUrl)
        {

            WebClient client = new WebClient();
            Stream stream = client.OpenRead(imageUrl);
            if (stream != null)
            {
                Bitmap bitmap = new Bitmap(stream);
                stream.Flush();
                stream.Close();
                return bitmap;
                
            }
            client.Dispose();
            return null;
        }

    }
}
