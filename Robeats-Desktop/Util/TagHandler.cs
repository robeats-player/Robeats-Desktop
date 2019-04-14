using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Robeats_Desktop.DataTypes;
using TagLib;

namespace Robeats_Desktop.Util
{
    public class TagHandler
    {
        public Song Song { get; set; }

        public TagLib.File TagFile { get; set; }
        public TagHandler(Song song)
        {
            Song = song;
            TagFile = TagLib.File.Create(Song.AbsolutePath);
        }

        public void SetTags()
        {
            if(Song.Title != null)
                TagFile.Tag.Title = Song.Title;
            if(Song.Album != null)
                TagFile.Tag.Album = Song.Album;
            if(Song.Artist != null)
                TagFile.Tag.Performers = new []{Song.Artist};
            TagFile.Save();
        }

        public void SetCover(Bitmap bitmap)
        {
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            var pic = new Picture
            {
                Type = PictureType.FrontCover,
                Description = "Cover",
                Data = ByteVector.FromStream(ms)
            };
            TagFile.Tag.Pictures = new IPicture[] { pic };
            TagFile.Save();
        }
    }
}