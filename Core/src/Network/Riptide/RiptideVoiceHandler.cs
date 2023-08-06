using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Utilities;


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnhollowerBaseLib;

using UnityEngine;
using UnityEngine.Rendering;
using PCMReaderCallback = UnityEngine.AudioClip.PCMReaderCallback;

namespace LabFusion.Core.src.Network.Riptide
{
    public class RiptideVoiceHandler : VoiceHandler
    {
        private const float _defaultVolumeMultiplier = 10f;

        private readonly MemoryStream _compressedVoiceStream = new();
        private MemoryStream _decompressedVoiceStream = new();
        private readonly Queue<float> _streamingReadQueue = new();

        public RiptideVoiceHandler(PlayerId id)
        {
            // Save the id
            _id = id;
            OnContactUpdated(ContactsList.GetContact(id));

            // Hook into contact info changing
            ContactsList.OnContactUpdated += OnContactUpdated;

            // Create the audio source and clip
            CreateAudioSource();

            Source.clip = AudioClip.Create("RiptideVoice", 44100,
            1, 44100, true, (PCMReaderCallback)PcmReaderCallback);

            _source.Play();

            // Set the rep's audio source
            VerifyRep();
        }

        public override void Cleanup()
        {
            // Unhook contact updating
            ContactsList.OnContactUpdated -= OnContactUpdated;

            base.Cleanup();
        }

        private void OnContactUpdated(Contact contact)
        {
            Volume = contact.volume;
        }

        public override void OnVoiceBytesReceived(byte[] bytes)
        {
            // TODO
        }

        private float GetVoiceMultiplier()
        {
            float mult = _defaultVolumeMultiplier * FusionPreferences.ClientSettings.GlobalVolume * Volume;

            // If we are loading or the audio is 2D, lower the volume
            if (FusionSceneManager.IsLoading() || _source.spatialBlend <= 0f)
            {
                mult *= 0.25f;
            }

            return mult;
        }

        private void PcmReaderCallback(Il2CppStructArray<float> data)
        {
            float mult = GetVoiceMultiplier();

            for (int i = 0; i < data.Length; i++)
            {
                if (_streamingReadQueue.Count > 0)
                {
                    data[i] = _streamingReadQueue.Dequeue() * mult;
                }
                else
                {
                    data[i] = 0.0f;  // Nothing in the queue means we should just play silence
                }
            }
        }
    }
}
