using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robeats_Desktop.DataTypes;
using Robeats_Desktop.Util;

namespace Robeats_DesktopTests
{
    [TestClass]
    public class DownloadTests
    {
        [TestMethod]
        public async Task DownloadVideoTest()
        {
            const string url = "https://www.youtube.com/watch?v=1lmpGxQnjqk&list=RD1lmpGxQnjqk&start_radio=1";
            var video = await Download.GetVideoAsync(url);
            var expected = video.Author;
            const string actual = "Vin Jay";
            Assert.IsNotNull(video);
            Assert.AreEqual(expected, actual);
        }
    }
}
