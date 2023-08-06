using MelonLoader;
using System;
using UnityEngine;

namespace LabFusion.MonoBehaviours
{
    [RegisterTypeInIl2Cpp]
    public class UnityVoice : MonoBehaviour
    {
        public UnityVoice(IntPtr intPtr) : base(intPtr) { }

        public bool isRecording => Microphone.IsRecording(Microphone.devices[0]);
        private int recordedSampleLength = 0;
        private AudioClip recordedClip;

        public void StartRecording()
        {
            if (!Microphone.IsRecording(Microphone.devices[0]))
            {
                recordedSampleLength = 0;
                recordedClip = Microphone.Start(Microphone.devices[0], true, 1, 44100);
            }
        }

        public void StopRecording()
        {
            if (Microphone.IsRecording(Microphone.devices[0]))
            {
                Microphone.End(Microphone.devices[0]);
            }
        }

        public bool IsMicrophoneRecording()
        {
            return isRecording;
        }

        public byte[] GetVoiceData()
        {
            if (recordedClip == null)
                return null;

            float[] samples = new float[recordedClip.samples];
            recordedClip.GetData(samples, 0);

            // Convert float samples to bytes (16-bit PCM)
            byte[] bytes = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short sampleValue = (short)(samples[i] * 32767f);
                bytes[i * 2] = (byte)(sampleValue & 0xFF);
                bytes[i * 2 + 1] = (byte)(sampleValue >> 8);
            }

            return bytes;
        }

        // Update is called once per frame
        void Update()
        {
            if (isRecording)
            {
                // Update the recorded sample length
                recordedSampleLength = Microphone.GetPosition(Microphone.devices[0]);
            }
        }
    }
}
