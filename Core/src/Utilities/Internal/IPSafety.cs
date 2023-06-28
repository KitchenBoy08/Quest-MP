using LabFusion.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

//Thank GOD for ChatGPT!

namespace LabFusion.IPSafety
{
    internal static class IPSafety
    {
        public static string EncodeIpAddress(string ipAddress)
        {
            string[] octets = ipAddress.Split('.');

            if (octets.Length != 4)
            {
                throw new ArgumentException("Invalid IP address format.");
            }

            byte[] binaryIp = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                if (!byte.TryParse(octets[i], out byte octet))
                {
                    throw new ArgumentException("Invalid IP address format.");
                }

                binaryIp[i] = octet;
            }

            return BitConverter.ToString(binaryIp).Replace("-", string.Empty);
        }

        public static string DecodeIpAddress(string encodedIp)
        {
            if (encodedIp.Length != 8)
            {
                throw new ArgumentException("Invalid encoded IP address length.");
            }

            byte[] binaryIp = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                string octetHex = encodedIp.Substring(i * 2, 2);
                if (!byte.TryParse(octetHex, System.Globalization.NumberStyles.HexNumber, null, out byte octet))
                {
                    throw new ArgumentException("Invalid encoded IP address format.");
                }

                binaryIp[i] = octet;
            }

            return string.Join(".", binaryIp);
        }
    }
}
