using System.Threading.Tasks;
using NUnit.Framework;
using Robeats_Desktop.Util;

namespace Robeats_DesktopTests
{
    [TestFixture]
    public class DownloadTests
    {
        [TestCase("https://www.youtube.com/watch?v=1lmpGxQnjqk&list=RD1lmpGxQnjqk&start_radio=1", "Vin Jay", "Vin Jay - Mumble Rapper vs Lyricist")]
        [TestCase("https://www.youtube.com/watch?v=rJOsjP33nF4&list=RD1lmpGxQnjqk&index=2", "BadMeetsEvilVEVO", "Bad Meets Evil - Fast Lane ft. Eminem, Royce Da 5'9")]
        public async Task DownloadVideoTest(string url, string authorExpected, string titleExpected)
        {
            var video = await Download.GetVideoAsync(url);

            var authorActual = video.Author;
            var titleActual = video.Title;

            Assert.AreEqual(authorExpected, authorActual);
            Assert.AreEqual(titleExpected, titleActual);
        }

    }
}
