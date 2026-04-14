using landerist_library.Pages;

namespace landerist_library.Downloaders
{
    public interface IDownloader
    {
        public string? Content { get; set; }

        public byte[]? Screenshot { get; set; }

        public short? HttpStatusCode { get; set; }

        public string? RedirectUrl { get; set; }

        public void Download(Page page);

    }
}
