namespace landerist_library.Infrastructure.Distribution;

internal interface IDistributionFileSystem
{
    void Copy(string sourcePath, string destinationPath, bool overwrite);

    bool Exists(string path);

    void Delete(string path);
}
