namespace landerist_library.Infrastructure.Distribution.FileSystem;

internal sealed class SystemDistributionFileSystem : IDistributionFileSystem
{
    public void Copy(string sourcePath, string destinationPath, bool overwrite) =>
        File.Copy(sourcePath, destinationPath, overwrite);

    public bool Exists(string path) => File.Exists(path);

    public void Delete(string path) => File.Delete(path);
}
