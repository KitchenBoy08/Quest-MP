using UnityEngine;
using Il2CppSystem;
using LabFusion.Utilities;
using LabFusion.Data;
using MelonLoader;
using System.Linq;
using System;
using BoneLib;

namespace LabFusion.MonoBehaviours
{
    [RegisterTypeInIl2Cpp]
    public class MicrophoneManager : MonoBehaviour
    {
        public MicrophoneManager(System.IntPtr intPtr) : base(intPtr) { }

        private string deviceName = null;
        private AudioClip microphoneClip = null;
        public bool isRecording = false;
        private int microphonePos = 0;

        public void ChangeMicrophone(string mic)
        {
            bool wasRecording = isRecording;
            StopMicrophone();
            deviceName = mic;
            if (wasRecording)
            {
                StartMicrophone();
            }
        }

        public void StartMicrophone(string selectedDevice = null, int lengthSeconds = 1)
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

        public void StopMicrophone()
        {
            if (!isRecording)
            {
                FusionLogger.Warn("Microphone is not recording.");
                return;
            }

            isRecording = false;
            Microphone.End(deviceName);
            microphonePos = 0;
        }

        public float[] GetMicrophoneData()
        {
            if (Microphone.GetPosition(deviceName) < microphonePos)
            {
                microphonePos = Microphone.GetPosition(deviceName);
            }

            if (!isRecording)
            {
                FusionLogger.Warn("Microphone is not recording.");
                StartMicrophone();
                return null;
            }

            float[] samples = new float[microphoneClip.samples];
            microphoneClip.GetData(samples, microphonePos);


            microphonePos = Microphone.GetPosition(deviceName);

            return samples;
        }
    }
}