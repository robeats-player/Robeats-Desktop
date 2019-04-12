using System;
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
    }
}
