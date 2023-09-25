using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSoftware.Utilities
{
    // This is useful for later :)
    public static class JsonUtils
    {
        public static async Task SaveFile(string fileName, string data)
        {
            System.IO.IsolatedStorage.IsolatedStorageFile local =
                System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();

            if (!local.DirectoryExists("MyDirectory"))
                local.CreateDirectory("MyDirectory");

            using (var isoFileStream =
                    new System.IO.IsolatedStorage.IsolatedStorageFileStream(
                        string.Format("MyDirectory\\{0}.txt", fileName),
                        System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite,
                            local))
            {
                using (var isoFileWriter = new System.IO.StreamWriter(isoFileStream))
                {
                    await isoFileWriter.WriteAsync(data);
                }
            }
        }

        public static async Task<string> LoadFile(string fileName)
        {
            string data;

            System.IO.IsolatedStorage.IsolatedStorageFile local =
                System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();

            using (var isoFileStream =
                    new System.IO.IsolatedStorage.IsolatedStorageFileStream
                        (string.Format("MyDirectory\\{0}.txt", fileName),
                        System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read,
                        local))
            {
                using (var isoFileReader = new System.IO.StreamReader(isoFileStream))
                {
                    data = await isoFileReader.ReadToEndAsync();
                }
            }

            return data;
        }
    }
}
