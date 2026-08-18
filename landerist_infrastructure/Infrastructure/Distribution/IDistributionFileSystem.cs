namespace landerist_library.Infrastructure.Distribution;

public interface IDistributionFileSystem
{
    void Copy(string sourcePath, string destinationPath, bool overwrite);

    bool Exists(string path);

    void Delete(string path);

    void CreateDirectory(string path);

    string ReadAllText(string path);

    void WriteAllText(string path, string contents);
}
