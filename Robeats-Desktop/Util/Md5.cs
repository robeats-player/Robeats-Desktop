using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Robeats_Desktop.Util
{
    static class Md5
    {
        public static byte[] Calculate(string fileName)
        {
            using(var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return md5.ComputeHash(stream);
                    //return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }


    }
}
