using LabFusion.Utilities;
using MelonLoader;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IdentityModel.Protocols.WSTrust;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;

public static class IPGetter
{
    public static void GetExternalIP(Action<string> onCompleted)
    {
        string ip = "";
        try
        {
            string link = "https://api.ipify.org?format=text";
            UnityWebRequest httpWebRequest = UnityWebRequest.Get(link);
            var requestSent = httpWebRequest.SendWebRequest();

            requestSent.m_completeCallback += new Action<UnityEngine.AsyncOperation>((op) =>
            {
                ip = httpWebRequest.downloadHandler.text;
                if (httpWebRequest.result == UnityWebRequest.Result.ConnectionError || httpWebRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    FusionLogger.Error(httpWebRequest.error);
                    onCompleted?.Invoke(ip);
                    return;
                }
                onCompleted?.Invoke(ip);
            });

        }
        catch (Exception e)
        {
            MelonLogger.Error($"Error when fetching external IP:");
            MelonLogger.Error(e);
        }
    }
}