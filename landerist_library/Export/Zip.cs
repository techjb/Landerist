using System.IO.Compression;

namespace landerist_library.Export
{
    public class Zip
    {
        public static bool Compress(string inputFile, string zipFile)
        {
            string[] files = new string[] { inputFile };
            return Compress(files, zipFile);
        }

        public static bool Compress(string[] files, string zipFile)
        {
            if (files.Length == 0)
            {
                return false;
            }

            File.Delete(zipFile);
            try
            {
                using FileStream zipFileStream = new(zipFile, FileMode.Create);
                using ZipArchive zipArchive = new(zipFileStream, ZipArchiveMode.Create);

                foreach (string file in files)
                {
                    if (!File.Exists(file))
                    {
                        return false;
                    }
                    ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(Path.GetFileName(file));
                    using FileStream fileStream = new(file, FileMode.Open);
                    using Stream stream = zipArchiveEntry.Open();
                    fileStream.CopyTo(stream);
                }
                return true;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("Compress", exception);
            }
            return false;
        }
    }
}
