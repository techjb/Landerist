namespace landerist_library.Downloaders
{
    public interface IDownloaderSessionFactory
    {
        public IDownloaderSession Create(bool useProxy);
    }
}
