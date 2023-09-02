using UnityEngine;
using System;
using LabFusion.Utilities;

public static class MicrophoneManager
{
    private static string deviceName = null;
    private static AudioClip microphoneClip = null;
    public static bool isRecording = false;
    private static int microphonePos;

    public static void StartMicrophone(string selectedDevice = null, int lengthSeconds = 1)
    {
        if (isRecording)
        {
            FusionLogger.Warn("Microphone is already recording.");
            return;
        }

        deviceName = selectedDevice;

        microphoneClip = Microphone.Start(deviceName, true, lengthSeconds, 44100);
        isRecording = true;
        microphonePos = 0;
    }

    public static void StopMicrophone()
    {
        if (!isRecording)
        {
            FusionLogger.Warn("Microphone is not recording.");
            return;
        }

        Microphone.End(deviceName);
        isRecording = false;
        microphonePos = 0;
    }

    public static byte[] GetMicrophoneData()
    {
        if (!isRecording)
        {
            FusionLogger.Warn("Microphone is not recording.");
            return null;
        }

        float[] samples = new float[microphoneClip.samples];
        microphoneClip.GetData(samples, microphonePos);

        // Convert float samples to byte array
        byte[] byteArray = new byte[samples.Length * 2]; // 2 bytes per sample
        Buffer.BlockCopy(samples, 0, byteArray, 0, byteArray.Length);

        microphonePos = Microphone.GetPosition(null);

        return byteArray;
    }
}
