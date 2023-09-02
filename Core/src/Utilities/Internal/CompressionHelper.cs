using System.IO;
using System.IO.Compression;

public static class CompressionHelper
{
    public static byte[] CompressByteArray(byte[] inputBytes)
    {
        using (MemoryStream compressedStream = new MemoryStream())
        {
            using (DeflateStream compressionStream = new DeflateStream(compressedStream, CompressionMode.Compress))
            {
                compressionStream.Write(inputBytes, 0, inputBytes.Length);
            }
            return compressedStream.ToArray();
        }
    }

    public static byte[] DecompressByteArray(byte[] compressedBytes)
    {
        using (MemoryStream decompressedStream = new MemoryStream())
        {
            using (MemoryStream compressedStream = new MemoryStream(compressedBytes))
            {
                using (DeflateStream decompressionStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(decompressedStream);
                }
            }
            return decompressedStream.ToArray();
        }
    }
}
