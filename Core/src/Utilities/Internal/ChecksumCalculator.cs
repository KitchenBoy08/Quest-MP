using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace ChecksumCalculator
{
    public class ChecksumCalculator
    {
        public static byte[] CalculateChecksum(string dllPath)
        {
            using (var fileStream = File.OpenRead(dllPath))
            {
                var crc32 = new CRC32();
                byte[] hash = crc32.ComputeHash(fileStream);
                return hash;
            }
        }
    }

    public class CRC32 : HashAlgorithm
    {
        private const uint DefaultPolynomial = 0xedb88320;
        private const uint DefaultSeed = 0xffffffff;

        private static uint[] defaultTable;

        private readonly uint seed;
        private readonly uint[] table;
        private uint hash;

        public CRC32()
        {
            table = InitializeTable(DefaultPolynomial);
            seed = DefaultSeed;
            Initialize();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            hash = CalculateHash(table, hash, array, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            var hashBuffer = UInt32ToBigEndianBytes(~hash);
            HashValue = hashBuffer;
            return hashBuffer;
        }

        public override void Initialize()
        {
            hash = seed;
        }

        private static uint[] InitializeTable(uint polynomial)
        {
            if (polynomial == DefaultPolynomial && defaultTable != null)
                return defaultTable;

            var createTable = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                var entry = (uint)i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                defaultTable = createTable;

            return createTable;
        }

        private static uint CalculateHash(uint[] table, uint seed, byte[] buffer, int start, int size)
        {
            var crc = seed;
            for (int i = start; i < size; i++)
                unchecked
                {
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                }
            return crc;
        }

        private static byte[] UInt32ToBigEndianBytes(uint value)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);
            return result;
        }
    }
}
