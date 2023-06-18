using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

//Yes, I know this isn't near the safest way to Encode/Decode the IP. Yes, I know it's easy to crack. Yes, I will fix it later, this is just a placeholder system.
//Yes, I know this is inefficient and practically useless. Yes, I know that the source code is public and anyone could see the encoder/decoder so all of this is fucking useless.

namespace LabFusion.IPSafety
{
    internal static class IPSafety
    {
        public static void EncodeIP(string decodedIP)
        {
            decodedIP = decodedIP.Replace(".", "/");
            decodedIP = decodedIP.Replace("0", "3");
            decodedIP = decodedIP.Replace("1", "4");
            decodedIP = decodedIP.Replace("2", "5");
            decodedIP = decodedIP.Replace("3", "6");
            decodedIP = decodedIP.Replace("4", "7");
            decodedIP = decodedIP.Replace("5", "8");
            decodedIP = decodedIP.Replace("6", "9");
            decodedIP = decodedIP.Replace("7", "0");
            decodedIP = decodedIP.Replace("8", "1");
            decodedIP = decodedIP.Replace("9", "2");

            string encodedIP = decodedIP;
            return;
        }

        public static void DecodeIP(string encodedIP)
        {
            encodedIP = encodedIP.Replace("/", ".");
            encodedIP = encodedIP.Replace("3", "0");
            encodedIP = encodedIP.Replace("4", "1");
            encodedIP = encodedIP.Replace("5", "2");
            encodedIP = encodedIP.Replace("6", "3");
            encodedIP = encodedIP.Replace("7", "4");
            encodedIP = encodedIP.Replace("8", "5");
            encodedIP = encodedIP.Replace("9", "6");
            encodedIP = encodedIP.Replace("0", "7");
            encodedIP = encodedIP.Replace("1", "8");
            encodedIP = encodedIP.Replace("2", "9");

            string decodedIP = encodedIP;
            return;
        }
    }
}
