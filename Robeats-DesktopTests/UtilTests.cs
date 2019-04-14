using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robeats_Desktop.DataTypes;

namespace Robeats_DesktopTests
{
    [TestClass]
    public class UtilTests
    {
        [TestMethod]
        public void PathUtil_SanitizeUrlTest()
        {
            var sanitizedString = PathUtil.Sanitize("Trap <Mix 2016> 💥Trap ? *& Future Bass Music Mix | Best EDM");
            Assert.AreEqual(sanitizedString, "Trap _Mix 2016_ 💥Trap _ _& Future Bass Music Mix _ Best EDM");
        }

        [TestMethod]
        public void ThumbnailDownloadTest()
        {
            //Download a random image
            var bitmap = Thumbnail.DownloadImage(
                "https://d1q6f0aelx0por.cloudfront.net/fa443219-42e0-4886-96b4-8733de694b72-c641a5d6-1ebf-44ee-9607-aef9b4ca1a3b-logo_large.png");
            Assert.IsNotNull(bitmap);
        }
    }
}
