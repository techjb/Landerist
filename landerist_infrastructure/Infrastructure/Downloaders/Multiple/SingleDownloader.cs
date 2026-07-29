using landerist_library.Infrastructure.Downloaders;
using landerist_library.Infrastructure.Downloaders.Puppeteer;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Downloaders.Multiple
{
    public class SingleDownloader
    {
        private readonly IDownloaderSessionFactory DownloaderSessionFactory;
        private IDownloaderSession Downloader;
        private bool Available;
        public int Id = 0;
        private int Chrashes = 0;
        private int Scraped = 0;
        private readonly bool UseProxy;

        public SingleDownloader(bool useProxy, IDownloaderSessionFactory downloaderSessionFactory)
        {
            UseProxy = useProxy;
            DownloaderSessionFactory = downloaderSessionFactory;
            Downloader = DownloaderSessionFactory.Create(UseProxy);
            Available = Downloader.BrowserInitialized();
        }

        private SingleDownloader(
            bool useProxy,
            IDownloaderSessionFactory downloaderSessionFactory,
            IDownloaderSession downloader)
        {
            UseProxy = useProxy;
            DownloaderSessionFactory = downloaderSessionFactory;
            Downloader = downloader;
            Available = Downloader.BrowserInitialized();
        }

        public static async Task<SingleDownloader> CreateAsync(
            bool useProxy,
            IDownloaderSessionFactory downloaderSessionFactory,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(downloaderSessionFactory);
            IDownloaderSession downloader = await downloaderSessionFactory
                .CreateAsync(useProxy, cancellationToken)
                .ConfigureAwait(false);
            return new SingleDownloader(
                useProxy,
                downloaderSessionFactory,
                downloader);
        }
        public bool TryReserve(bool useProxy)
        {
            if (!Available || useProxy != UseProxy)
            {
                return false;
            }

            Available = false;
            return true;
        }

        private void Release()
        {
            Available = true;
        }

        public bool GetUseProxy()
        {
            return UseProxy;
        }

        public bool Download(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            var restartedBrowser = false;

            try
            {
                Downloader.Download(page);
                Scraped++;

                if (BrowserHasChrashed())
                {
                    Chrashes++;
                    RestartBrowser();
                    restartedBrowser = true;
                    return false;
                }

                return true;
            }
            catch //(Exception ex)
            {
                Chrashes++;
                RestartBrowser();
                restartedBrowser = true;
                return false;
            }
            finally
            {
                if (!restartedBrowser)
                {
                    Release();
                }
            }
        }

        public async Task<bool> DownloadAsync(
            Page page,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);
            bool restartedBrowser = false;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Downloader.DownloadAsync(page, cancellationToken)
                    .ConfigureAwait(false);
                Scraped++;

                if (BrowserHasChrashed())
                {
                    Chrashes++;
                    restartedBrowser = true;
                    await RestartBrowserAsync(cancellationToken)
                        .ConfigureAwait(false);
                    return false;
                }

                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                Chrashes++;
                restartedBrowser = true;
                await RestartBrowserAsync(cancellationToken)
                    .ConfigureAwait(false);
                return false;
            }
            finally
            {
                if (!restartedBrowser)
                {
                    Release();
                }
            }
        }
        public void CloseBrowser()
        {
            Downloader.CloseBrowser();
            Available = false;
        }

        public async Task CloseBrowserAsync()
        {
            await Downloader.CloseBrowserAsync().ConfigureAwait(false);
            Available = false;
        }

        public async Task RestartBrowserAsync(
            CancellationToken cancellationToken = default)
        {
            await CloseBrowserAsync().ConfigureAwait(false);
            Downloader = await DownloaderSessionFactory
                .CreateAsync(UseProxy, cancellationToken)
                .ConfigureAwait(false);
            Available = Downloader.BrowserInitialized();
        }
        public bool BrowserHasChrashed()
        {
            return Downloader.BrowserHasChrashed();
        }

        public void RestartBrowser()
        {
            CloseBrowser();
            Downloader = DownloaderSessionFactory.Create(UseProxy);
            Available = Downloader.BrowserInitialized();
        }

        public int ScrapedCounter()
        {
            return Scraped;
        }

        public int CrashesCounter()
        {
            return Chrashes;
        }
    }
}
