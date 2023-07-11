using LabFusion.Extensions;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Utilities;
using Riptide;
using Riptide.Transports;
using Riptide.Utils;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Media;

using UnhollowerBaseLib;

using UnityEngine;

using PCMReaderCallback = UnityEngine.AudioClip.PCMReaderCallback;
using System.Runtime.InteropServices;

namespace LabFusion.Network
{
    public class RiptideVoiceIdentifier
    {
        public static List<RiptideVoiceIdentifier> VoiceIdentifiers = new List<RiptideVoiceIdentifier>();

        private const float _defaultVolumeMultiplier = 10f;

        private readonly MemoryStream _compressedVoiceStream = new MemoryStream();
        private readonly MemoryStream _decompressedVoiceStream = new MemoryStream();
        private readonly Queue<float> _streamingReadQueue = new Queue<float>();

        private AudioSource _source;
        private PlayerId _id;
        private PlayerRep _rep;
        private bool _hasRep;

        private float _lastClearTime;

        public RiptideVoiceIdentifier(PlayerId id)
        {
            // Create the audio source and clip
            _source = new GameObject($"{id.SmallId} Voice Source").AddComponent<AudioSource>();
            GameObject.DontDestroyOnLoad(_source);
            GameObject.DontDestroyOnLoad(_source.gameObject);
            _source.gameObject.hideFlags = HideFlags.DontUnloadUnusedAsset;

            _source.clip = AudioClip.Create("RiptideVoice", Convert.ToInt32(44100),
                        1, Convert.ToInt32(44100), true, (PCMReaderCallback)PcmReaderCallback);

            // Setup the mixing settings
            _source.rolloffMode = AudioRolloffMode.Linear;

            // Set it to loop and play so its constantly active
            _source.loop = true;
            _source.Play();

            // Save values
            _id = id;
            VerifyPlayerRep();

            // Add to list
            VoiceIdentifiers.Add(this);
        }

        public void Cleanup()
        {
            // Destroy audio source
            if (!_source.IsNOC())
            {
                // Get rid of clip
                if (!_source.clip.IsNOC())
                    GameObject.Destroy(_source.clip);

                GameObject.Destroy(_source.gameObject);
            }
        }

        public static void CleanupAll()
        {
            foreach (var identifer in VoiceIdentifiers.ToArray())
            {
                identifer.Cleanup();
            }
        }

        public static RiptideVoiceIdentifier GetVoiceIdentifier(PlayerId id)
        {
            for (var i = 0; i < VoiceIdentifiers.Count; i++)
            {
                var identifier = VoiceIdentifiers[i];

                if (identifier._id == id)
                    return identifier;
            }

            var newIdentifier = new RiptideVoiceIdentifier(id);
            return newIdentifier;
        }

        private void VerifyPlayerRep()
        {
            if (_id != null && !_hasRep)
            {
                PlayerRepManager.TryGetPlayerRep(_id, out _rep);

                if (_rep != null)
                {
                    _rep.InsertVoiceSource(_source);
                    _hasRep = true;
                }
            }
        }

        public void OnVoiceBytesReceived(byte[] bytes)
        {
            VerifyPlayerRep();

            // Decompress the voice data
            _compressedVoiceStream.Position = 0;
            _compressedVoiceStream.Write(bytes, 0, bytes.Length);

            _compressedVoiceStream.Position = 0;
            _decompressedVoiceStream.Position = 0;
            int numBytesWritten;

            byte[] decompressedData = DecompressVoiceStream(_compressedVoiceStream, out numBytesWritten);
            _decompressedVoiceStream.Write(decompressedData, 0, decompressedData.Length);

            _decompressedVoiceStream.Position = 0;

            while (_decompressedVoiceStream.Position < numBytesWritten)
            {
                byte byte1 = (byte)_decompressedVoiceStream.ReadByte();
                byte byte2 = (byte)_decompressedVoiceStream.ReadByte();

                short pcmShort = (short)((byte2 << 8) | (byte1 << 0));
                float pcmFloat = Convert.ToSingle(pcmShort) / short.MaxValue;

                _streamingReadQueue.Enqueue(pcmFloat);
            }

            // Reset clear time since we received a message
            _lastClearTime = Time.realtimeSinceStartup;
        }

        private float GetVoiceMultiplier()
        {
            float mult = _defaultVolumeMultiplier * FusionPreferences.ClientSettings.GlobalVolume;

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

        public static byte[] DecompressVoiceStream(MemoryStream compressedStream, out int numBytesWritten)
        {
            // Create a decompression stream using the compressed memory stream
            using (var decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                // Create a memory stream to hold the decompressed data
                var decompressedData = new MemoryStream();

                // Decompress the voice stream
                decompressionStream.CopyTo(decompressedData);
                decompressedData.Seek(0, SeekOrigin.Begin);

                // Get bytes
                byte[] bytes = decompressedData.GetBuffer();

                numBytesWritten = Convert.ToInt32(bytes);


                return decompressedData.ToArray();
            }
        }
    }
}
