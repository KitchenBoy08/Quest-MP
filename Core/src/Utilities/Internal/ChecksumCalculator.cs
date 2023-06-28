using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace LabFusion.Core.src.Utilities.Internal
{
    internal class ChecksumCalculator
    {
        public static string CalculateChecksum(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    byte[] checksum = sha256.ComputeHash(fileStream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty);
                }
            }
        }
    }
}
