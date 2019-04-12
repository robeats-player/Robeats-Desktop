using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robeats_Desktop;
using Robeats_Desktop.DataTypes;

namespace Robeats_DesktopTests
{
    [TestClass]
    public class ThumbnailTests
    {
        [TestMethod]
        public void DownloadThumbnailTest()
        {
            Thumbnail.DownloadImage("https://www.pdr.nl/uploads/img/IMG18-08-22_16-47-17.png");
        }


    }
}
