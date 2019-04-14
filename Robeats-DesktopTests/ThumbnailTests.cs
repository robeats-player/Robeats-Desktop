using System;
using System.Drawing;
using NUnit.Framework;
using Robeats_Desktop;
using Robeats_Desktop.DataTypes;

namespace Robeats_DesktopTests
{
    [TestFixture]
    public class ThumbnailTests
    {
        [Test]
        public void DownloadThumbnailTest()
        {
            Thumbnail.DownloadImage("https://www.pdr.nl/uploads/img/IMG18-08-22_16-47-17.png");
        }


    }
}
