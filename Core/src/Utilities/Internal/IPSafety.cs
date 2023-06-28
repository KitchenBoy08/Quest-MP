using Il2CppSystem.Text;
using Il2CppSystem;
using LabFusion.MonoBehaviours;
using MelonLoader.ICSharpCode.SharpZipLib.Checksum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using System.Net;
using MelonLoader;

//Thank GOD for ChatGPT!

namespace LabFusion.IPSafety
{
    internal static class IPSafety
    {
        public static byte[] EncodePacket(string ipAddress, byte[] checksum)
        {
            // Convert IP address to byte array
            byte[] ipBytes = System.Text.Encoding.ASCII.GetBytes(ipAddress);

            // Create encoded packet
            byte[] encodedPacket = new byte[ipBytes.Length + checksum.Length];
            System.Buffer.BlockCopy(ipBytes, 0, encodedPacket, 0, ipBytes.Length);
            System.Buffer.BlockCopy(checksum, 0, encodedPacket, ipBytes.Length, checksum.Length);

            return encodedPacket;
        }

        public static string DecodePacket(byte[] encodedPacket, int checksumLength)
        {
            // Extract IP address
            byte[] ipBytes = new byte[encodedPacket.Length - checksumLength];
            System.Buffer.BlockCopy(encodedPacket, 0, ipBytes, 0, ipBytes.Length);
            string ipAddress = System.Text.Encoding.ASCII.GetString(ipBytes);

            return ipAddress;
        }

        public static string GetPublicIP()
        {
            string apiUrl = "https://api.ipify.org"; // API endpoint for retrieving public IP
            string result = string.Empty;

            try
            {
                using (WebClient client = new WebClient())
                {
                    result = client.DownloadString(apiUrl);
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Msg("An error occurred while retrieving the public IP: " + ex.Message);
            }

            return result.Trim(); // Trim any whitespace characters
        }
    }
}
