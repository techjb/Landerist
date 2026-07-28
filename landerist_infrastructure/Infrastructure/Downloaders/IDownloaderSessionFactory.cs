namespace landerist_library.Infrastructure.Downloaders
{
    public interface IDownloaderSessionFactory
    {
        public IDownloaderSession Create(bool useProxy);
    }
}
