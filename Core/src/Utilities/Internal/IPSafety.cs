﻿using Il2CppSystem.Text;
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
    }
}
