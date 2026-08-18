using System.Globalization;
using System.Net;
using System.Text;
using landerist_library.Application.Websites;
using landerist_library.Application.Distribution;
using landerist_library.Database;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Websites;
using landerist_orels.ES;
using static landerist_library.Infrastructure.Distribution.DistributionArtifactNaming;

namespace landerist_library.Infrastructure.Distribution
{
    public sealed class DownloadsPage
    {
        private readonly IDistributionWebsiteMetrics _websiteMetrics;
        private readonly IWebsiteCatalog _websites;
        private readonly DistributionOptions _options;
        private readonly IWebsiteArtifactStorage _storage;
        private readonly IDistributionFileSystem _files;
        private readonly string DownloadsTemplateHtmlFile;
        private readonly string DownloadsIndexHtmlFile;

        public DownloadsPage(
            IDistributionWebsiteMetrics websiteMetrics,
            IWebsiteCatalog websites,
            DistributionOptions options,
            IWebsiteArtifactStorage storage,
            IDistributionFileSystem files)
        {
            ArgumentNullException.ThrowIfNull(websiteMetrics);
            ArgumentNullException.ThrowIfNull(websites);
            ArgumentNullException.ThrowIfNull(storage);
            ArgumentNullException.ThrowIfNull(files);
            _websiteMetrics = websiteMetrics;
            _websites = websites;
            _options = options;
            _storage = storage;
            _files = files;
            DownloadsTemplateHtmlFile = Path.Combine(options.TemplatesDirectory, "downloads", "downloads_template.html");
            DownloadsIndexHtmlFile = Path.Combine(options.TemplatesDirectory, "downloads", "index.html");
        }

        private string DownloadsTemplate = string.Empty;

        public void Update()
        {
            try
            {
                string downloadsTemplate = _files.ReadAllText(DownloadsTemplateHtmlFile);

                if (UploadDownloadsIndexFile())
                {
                    Log.WriteInfo("DownloadsPage", "Updated downloads index page");
                }

                foreach (var country in landerist_library.Websites.Countries.All)
                {
                    DownloadsTemplate = downloadsTemplate;
                    UpdateCountryName(country);
                    UpdateUpdatedAt();

                    foreach (ExportType exportType in GetDownloadsExportTypes())
                    {
                        UpdateDownloadsTemplate(country.CountryCode, exportType);
                    }

                    UpdateListingsByOperationPropertyTypeTemplate(country.CountryCode);
                    UpdateHostsTemplate(country.CountryCode);

                    if (UploadDownloadsFile(country.CountryCode))
                    {
                        Log.WriteInfo("DownloadsPage", $"Updated downloads page for {country.CountryCode}");
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("DownloadsPage Update", exception);
            }
        }

        private void UpdateCountryName(Country country)
        {
            Replace("/*COUNTRY_NAME*/", WebUtility.HtmlEncode(country.CountryName));
        }

        private void UpdateUpdatedAt()
        {
            string updatedAtText = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            Replace("/*UPDATED_AT*/", updatedAtText);
        }

        private ExportType[] GetDownloadsExportTypes()
        {
            return
            [
                ExportType.Published,
                ExportType.Unpublished,
                ExportType.PublishedUpdates,
                ExportType.UnpublishedUpdates
            ];
        }

        private void UpdateDownloadsTemplate(CountryCode countryCode, ExportType exportType)
        {
            string objectKey = GetObjectKey(countryCode, exportType, "json");

            var (lastModified, contentLength) = _storage.GetFileInfo(_options.DownloadsBucket, objectKey);
            if (lastModified is null || contentLength is null)
            {
                return;
            }

            var counter = _storage.GetMetadata(
                _options.DownloadsBucket,
                objectKey,
                DownloadsUpdater.METADATA_KEY_COUNTER);

            string sizeString = FormatBytes((long)contentLength);
            Replace(Comment(countryCode, exportType, "Size"), sizeString);

            var url = $"https://{_options.DownloadsBucket}.s3.amazonaws.com/{objectKey}";
            string fileName = GetFileName(countryCode, exportType, "json");
            string counterText = counter ?? "-";
            string counterHyperlink = $"<a title=\"Download\" href=\"{WebUtility.HtmlEncode(url)}\" download=\"{WebUtility.HtmlEncode(fileName)}\">{WebUtility.HtmlEncode(counterText)}</a>";
            Replace(Comment(countryCode, exportType, "Counter"), counterHyperlink);
        }

        private void UpdateHostsTemplate(CountryCode countryCode)
        {
            Replace("/*HOSTS*/", GetHostsTableRows(countryCode));
        }

        private void UpdateListingsByOperationPropertyTypeTemplate(CountryCode countryCode)
        {
            Replace("/*LISTINGS_BY_OPERATION_PROPERTY_TYPE*/", GetListingsByOperationPropertyTypeTableRows(countryCode));
        }

        private string GetListingsByOperationPropertyTypeTableRows(CountryCode countryCode)
        {
            StringBuilder rows = new();

            foreach (Operation operation in Enum.GetValues<Operation>())
            {
                foreach (PropertyType propertyType in Enum.GetValues<PropertyType>())
                {
                    int publishedListingsCount = _websiteMetrics.CountListings(
                        ListingStatus.published,
                        operation,
                        propertyType);

                    int unpublishedListingsCount = _websiteMetrics.CountListings(
                        ListingStatus.unpublished,
                        operation,
                        propertyType);

                    rows.AppendLine(GetListingsByOperationPropertyTypeTableRow(
                        countryCode,
                        operation,
                        propertyType,
                        publishedListingsCount,
                        unpublishedListingsCount));
                }
            }

            return rows.ToString();
        }

        private string GetListingsByOperationPropertyTypeTableRow(
            CountryCode countryCode,
            Operation operation,
            PropertyType propertyType,
            int publishedListingsCount,
            int unpublishedListingsCount)
        {
            string label = $"{operation} {propertyType}";

            return
                "                <tr>" + Environment.NewLine +
                $"                    <td>{WebUtility.HtmlEncode(label)}</td>" + Environment.NewLine +
                $"                    <td>{GetListingsByOperationPropertyTypeDownloadCellText(countryCode, operation, propertyType, ListingStatus.published, publishedListingsCount)}</td>" + Environment.NewLine +
                $"                    <td>{GetListingsByOperationPropertyTypeDownloadCellText(countryCode, operation, propertyType, ListingStatus.unpublished, unpublishedListingsCount)}</td>" + Environment.NewLine +
                "                </tr>";
        }

        private string GetListingsByOperationPropertyTypeDownloadCellText(
            CountryCode countryCode,
            Operation operation,
            PropertyType propertyType,
            ListingStatus listingStatus,
            int counter)
        {
            string counterText = counter.ToString(CultureInfo.InvariantCulture);
            if (counter <= 0)
            {
                return counterText;
            }

            string? url = GetListingsByOperationPropertyTypeDownloadUrl(countryCode, operation, propertyType, listingStatus);
            if (string.IsNullOrWhiteSpace(url))
            {
                return counterText;
            }

            string fileName = GetListingsByOperationPropertyTypeFileName(countryCode, operation, propertyType, listingStatus, "json");
            return $"<a title=\"Download\" href=\"{WebUtility.HtmlEncode(url)}\" download=\"{WebUtility.HtmlEncode(fileName)}\">{counterText}</a>";
        }

        private string? GetListingsByOperationPropertyTypeDownloadUrl(
            CountryCode countryCode,
            Operation operation,
            PropertyType propertyType,
            ListingStatus listingStatus)
        {
            string objectKey = GetListingsByOperationPropertyTypeObjectKey(countryCode, operation, propertyType, listingStatus, "json");
            var (lastModified, contentLength) = _storage.GetFileInfo(_options.DownloadsBucket, objectKey);

            if (lastModified is null || contentLength is null)
            {
                return null;
            }

            return $"https://{_options.DownloadsBucket}.s3.amazonaws.com/{objectKey}";
        }

        private string GetListingsByOperationPropertyTypeObjectKey(
            CountryCode countryCode,
            Operation operation,
            PropertyType propertyType,
            ListingStatus listingStatus,
            string extension)
        {
            return $"{countryCode}/OperationPropertyTypes/{GetListingsByOperationPropertyTypeFileName(countryCode, operation, propertyType, listingStatus, extension)}";
        }

        private string GetHostsTableRows(CountryCode countryCode)
        {
            StringBuilder rows = new();

            foreach (var website in _websites.GetAll()
                .Where(website => website.CountryCode == countryCode)
                .OrderBy(website => website.Host, StringComparer.OrdinalIgnoreCase))
            {
                int publishedListingsCount = _websiteMetrics.CountPublishedListings(website);
                int unpublishedListingsCount = _websiteMetrics.CountUnpublishedListings(website);

                rows.AppendLine(GetHostTableRow(
                    countryCode,
                    website,
                    publishedListingsCount,
                    unpublishedListingsCount));
            }

            return rows.ToString();
        }

        private string GetHostTableRow(
            CountryCode countryCode,
            Website website,
            int publishedListingsCount,
            int unpublishedListingsCount)
        {
            return
                "                <tr>" + Environment.NewLine +
                $"                    <td>{WebUtility.HtmlEncode(website.Host)}</td>" + Environment.NewLine +
                $"                    <td>{GetHostDownloadCellText(countryCode, website.Host, ListingStatus.published, "json", publishedListingsCount)}</td>" + Environment.NewLine +
                $"                    <td>{GetHostDownloadCellText(countryCode, website.Host, ListingStatus.unpublished, "json", unpublishedListingsCount)}</td>" + Environment.NewLine +
                "                </tr>";
        }

        private string GetHostDownloadCellText(CountryCode countryCode, string host, ListingStatus listingStatus, string extension, int counter)
        {
            string counterText = counter.ToString(CultureInfo.InvariantCulture);
            if (counter <= 0)
            {
                return counterText;
            }

            string? url = GetHostDownloadUrl(countryCode, host, listingStatus, extension);
            if (string.IsNullOrWhiteSpace(url))
            {
                return counterText;
            }

            string fileName = GetHostListingsFileName(countryCode, host, listingStatus, extension);
            return $"<a title=\"Download\" href=\"{WebUtility.HtmlEncode(url)}\" download=\"{WebUtility.HtmlEncode(fileName)}\">{counterText}</a>";
        }

        private string? GetHostDownloadUrl(CountryCode countryCode, string host, ListingStatus listingStatus, string extension)
        {
            string objectKey = GetHostObjectKey(countryCode, host, listingStatus, extension);
            var (lastModified, contentLength) = _storage.GetFileInfo(_options.DownloadsBucket, objectKey);

            if (lastModified is null || contentLength is null)
            {
                return null;
            }

            return $"https://{_options.DownloadsBucket}.s3.amazonaws.com/{objectKey}";
        }

        private string GetHostObjectKey(CountryCode countryCode, string host, ListingStatus listingStatus, string extension)
        {
            return $"{countryCode}/Hosts/{GetHostListingsFileName(countryCode, host, listingStatus, extension)}";
        }

        private string Comment(CountryCode countryCode, ExportType exportType, string key)
        {
            return $"<!--{countryCode}_{exportType}_{key}-->";
        }

        private void Replace(string comment, string? text)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return;
            }

            DownloadsTemplate = DownloadsTemplate.Replace(comment, text ?? string.Empty);
        }

        private bool UploadDownloadsFile(CountryCode countryCode)
        {
            string downloadsHtmlFile = GetDownloadsHtmlFile(countryCode);
            _files.CreateDirectory(Path.GetDirectoryName(downloadsHtmlFile)!);
            _files.WriteAllText(downloadsHtmlFile, DownloadsTemplate);

            return _storage.Upload(downloadsHtmlFile, "index.html", GetDownloadsWebsiteDirectory(countryCode));
        }

        private bool UploadDownloadsIndexFile()
        {
            return _storage.Upload(DownloadsIndexHtmlFile, "index.html", "downloads");
        }

        private string GetDownloadsHtmlFile(CountryCode countryCode)
        {
            return Path.Combine(_options.OutputDirectory, GetDownloadsWebsiteDirectory(countryCode), "index.html");
        }

        private string GetDownloadsWebsiteDirectory(CountryCode countryCode)
        {
            return $"downloads/{countryCode.ToString().ToLowerInvariant()}";
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

            if (bytes == 0)
            {
                return "0 B";
            }

            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
    }
}
