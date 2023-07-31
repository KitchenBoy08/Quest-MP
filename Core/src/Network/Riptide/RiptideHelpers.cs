using BoneLib.BoneMenu.Elements;
using Il2CppSystem;
using LabFusion.Network;
using LabFusion.Preferences;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Il2CppSystem.Linq.Expressions.Interpreter.CastInstruction.CastInstructionNoT;

namespace LabFusion.Core.src.Network.Riptide
{
    public class RiptideHelpers
    {
        private static byte[][] BufferPool;
        private static int BufferPoolIndex;
        public static byte[] TakeBuffer(int minSize)
        {
            if (BufferPool == null)
            {
                //
                // The pool has 8 items.
                //
                BufferPool = new byte[8][];

                for (int i = 0; i < BufferPool.Length; i++)
                    BufferPool[i] = new byte[1024 * 128];
            }

            BufferPoolIndex++;
            if (BufferPoolIndex >= BufferPool.Length)
                BufferPoolIndex = 0;

            if (BufferPool[BufferPoolIndex].Length < minSize)
            {
                BufferPool[BufferPoolIndex] = new byte[minSize + 1024];
            }

            return BufferPool[BufferPoolIndex];
        }
        public static unsafe int DecompressVoice(MemoryStream input, int length, out MemoryStream output)
        {
            {
                output = new MemoryStream();

                var from = TakeBuffer(length);
                var to = TakeBuffer(1024 * 64);

                //
                // Copy from input stream to a pinnable buffer
                //
                using (var s = new System.IO.MemoryStream(from))
                {
                    input.CopyTo(s);
                }

                uint szWritten = 0;

                output.Write(to, 0, (int)szWritten);
                return (int)output.Length;
            }
        }
    }
}
