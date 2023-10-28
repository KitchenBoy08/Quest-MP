using System;
using Il2CppSystem;
using LabFusion.Utilities;
using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;

public static class IPGetter
{
    public static void GetExternalIP(System.Action<string> onCompleted)
    {
        string ip = "";
        try
        {
            string link = "https://api.ipify.org?format=text";
            UnityWebRequest httpWebRequest = UnityWebRequest.Get(link);
            var requestSent = httpWebRequest.SendWebRequest();

            requestSent.m_completeCallback += new System.Action<UnityEngine.AsyncOperation>((op) =>
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
        catch (System.Exception e)
        {
            MelonLogger.Error($"Error when fetching external IP:");
            MelonLogger.Error(e);
        }
    }
}
