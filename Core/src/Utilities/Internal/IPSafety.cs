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
using BoneLib;
using UnityEngine.Networking;
using System.IO;


//Thank GOD for ChatGPT!

namespace LabFusion.IPSafety
{
    internal static class IPSafety
    {
        public static string EncodeIPAddress(string ipAddress)
        {
            string[] parts = ipAddress.Split('.');
            int part1 = int.Parse(parts[0]);
            int part2 = int.Parse(parts[1]);
            int part3 = int.Parse(parts[2]);
            int part4 = int.Parse(parts[3]);

            int encodedValue = (part1 << 24) | (part2 << 16) | (part3 << 8) | part4;

            return System.Convert.ToBase64String(System.BitConverter.GetBytes(encodedValue));
        }

        public static string DecodeIPAddress(string encodedIPAddress)
        {
            byte[] bytes = System.Convert.FromBase64String(encodedIPAddress);
            int encodedValue = System.BitConverter.ToInt32(bytes, 0);

            int part1 = (encodedValue >> 24) & 255;
            int part2 = (encodedValue >> 16) & 255;
            int part3 = (encodedValue >> 8) & 255;
            int part4 = encodedValue & 255;

            return $"{part1}.{part2}.{part3}.{part4}";
        }

        public static string GetPublicIP()
        {
            string apiUrl = "https://api.ipify.org"; // API endpoint for retrieving public IP
            string result = string.Empty;

            if (!HelperMethods.IsAndroid())
            {

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
            else
            {

                WebRequest request = WebRequest.Create(apiUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream dataStream = response.GetResponseStream();

                using StreamReader reader = new StreamReader(dataStream);

                var ip = reader.ReadToEnd();
                reader.Close();

                return ip;
            }
        }
    }
}
