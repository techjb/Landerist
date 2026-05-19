using System.Globalization;
using System.Net;
using System.Text;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Landerist_com
{
    public class DownloadsPage : Landerist_com
    {
        private static readonly string DownloadsTemplateHtmlFile =
            Path.Combine(Config.LANDERIST_COM_TEMPLATES!, "downloads", "downloads_template.html");

        private static readonly string DownloadsIndexHtmlFile =
            Path.Combine(Config.LANDERIST_COM_TEMPLATES!, "downloads", "index.html");

        private static string DownloadsTemplate = string.Empty;

        public static void Update()
        {
            try
            {
                string downloadsTemplate = File.ReadAllText(DownloadsTemplateHtmlFile);

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

        private static void UpdateCountryName(Country country)
        {
            Replace("/*COUNTRY_NAME*/", WebUtility.HtmlEncode(country.CountryName));
        }

        private static void UpdateUpdatedAt()
        {
            string updatedAtText = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            Replace("/*UPDATED_AT*/", updatedAtText);
        }

        private static ExportType[] GetDownloadsExportTypes()
        {
            return
            [
                ExportType.Published,
                ExportType.Unpublished,
                ExportType.PublishedUpdates,
                ExportType.UnpublishedUpdates
            ];
        }

        private static void UpdateDownloadsTemplate(CountryCode countryCode, ExportType exportType)
        {
            var s3 = new S3();
            string objectKey = GetObjectKey(countryCode, exportType, "json");

            var (lastModified, contentLength) = s3.GetFileInfo(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey);
            if (lastModified is null || contentLength is null)
            {
                return;
            }

            var counter = s3.GetMetadataValue(
                PrivateConfig.AWS_S3_DOWNLOADS_BUCKET,
                objectKey,
                DownloadsUpdater.METADATA_KEY_COUNTER);

            string sizeString = FormatBytes((long)contentLength);
            Replace(Comment(countryCode, exportType, "Size"), sizeString);

            var url = $"https://{PrivateConfig.AWS_S3_DOWNLOADS_BUCKET}.s3.amazonaws.com/{objectKey}";
            string fileName = GetFileName(countryCode, exportType, "json");
            string counterText = counter ?? "-";
            string counterHyperlink = $"<a title=\"Download\" href=\"{WebUtility.HtmlEncode(url)}\" download=\"{WebUtility.HtmlEncode(fileName)}\">{WebUtility.HtmlEncode(counterText)}</a>";
            Replace(Comment(countryCode, exportType, "Counter"), counterHyperlink);
        }

        private static void UpdateHostsTemplate(CountryCode countryCode)
        {
            Replace("/*HOSTS*/", GetHostsTableRows(countryCode));
        }

        private static void UpdateListingsByOperationPropertyTypeTemplate(CountryCode countryCode)
        {
            Replace("/*LISTINGS_BY_OPERATION_PROPERTY_TYPE*/", GetListingsByOperationPropertyTypeTableRows(countryCode));
        }

        private static string GetListingsByOperationPropertyTypeTableRows(CountryCode countryCode)
        {
            StringBuilder rows = new();

            foreach (Operation operation in Enum.GetValues<Operation>())
            {
                foreach (PropertyType propertyType in Enum.GetValues<PropertyType>())
                {
                    int publishedListingsCount = ES_Listings.CountApplySpecialRules(
                        ListingStatus.published,
                        operation,
                        propertyType);

                    int unpublishedListingsCount = ES_Listings.CountApplySpecialRules(
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

        private static string GetListingsByOperationPropertyTypeTableRow(
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

        private static string GetListingsByOperationPropertyTypeDownloadCellText(
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

        private static string? GetListingsByOperationPropertyTypeDownloadUrl(
            CountryCode countryCode,
            Operation operation,
            PropertyType propertyType,
            ListingStatus listingStatus)
        {
            string objectKey = GetListingsByOperationPropertyTypeObjectKey(countryCode, operation, propertyType, listingStatus, "json");
            var (lastModified, contentLength) = new S3().GetFileInfo(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey);

            if (lastModified is null || contentLength is null)
            {
                return null;
            }

            return $"https://{PrivateConfig.AWS_S3_DOWNLOADS_BUCKET}.s3.amazonaws.com/{objectKey}";
        }

        private static string GetListingsByOperationPropertyTypeObjectKey(
            CountryCode countryCode,
            Operation operation,
            PropertyType propertyType,
            ListingStatus listingStatus,
            string extension)
        {
            return $"{countryCode}/OperationPropertyTypes/{GetListingsByOperationPropertyTypeFileName(countryCode, operation, propertyType, listingStatus, extension)}";
        }

        private static string GetHostsTableRows(CountryCode countryCode)
        {
            StringBuilder rows = new();

            foreach (var website in Websites.Websites.GetApplySpecialRules()
                .Where(website => website.CountryCode == countryCode)
                .OrderBy(website => website.Host, StringComparer.OrdinalIgnoreCase))
            {
                int publishedListingsCount = website.GetNumPublishedListings();
                int unpublishedListingsCount = website.GetNumUnpublishedListings();

                rows.AppendLine(GetHostTableRow(
                    countryCode,
                    website,
                    publishedListingsCount,
                    unpublishedListingsCount));
            }

            return rows.ToString();
        }

        private static string GetHostTableRow(
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

        private static string GetHostDownloadCellText(CountryCode countryCode, string host, ListingStatus listingStatus, string extension, int counter)
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

        private static string? GetHostDownloadUrl(CountryCode countryCode, string host, ListingStatus listingStatus, string extension)
        {
            string objectKey = GetHostObjectKey(countryCode, host, listingStatus, extension);
            var (lastModified, contentLength) = new S3().GetFileInfo(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey);

            if (lastModified is null || contentLength is null)
            {
                return null;
            }

            return $"https://{PrivateConfig.AWS_S3_DOWNLOADS_BUCKET}.s3.amazonaws.com/{objectKey}";
        }

        private static string GetHostObjectKey(CountryCode countryCode, string host, ListingStatus listingStatus, string extension)
        {
            return $"{countryCode}/Hosts/{GetHostListingsFileName(countryCode, host, listingStatus, extension)}";
        }

        private static string Comment(CountryCode countryCode, ExportType exportType, string key)
        {
            return $"<!--{countryCode}_{exportType}_{key}-->";
        }

        private static void Replace(string comment, string? text)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return;
            }

            DownloadsTemplate = DownloadsTemplate.Replace(comment, text ?? string.Empty);
        }

        private static bool UploadDownloadsFile(CountryCode countryCode)
        {
            string downloadsHtmlFile = GetDownloadsHtmlFile(countryCode);
            Directory.CreateDirectory(Path.GetDirectoryName(downloadsHtmlFile)!);
            File.WriteAllText(downloadsHtmlFile, DownloadsTemplate);

            return new S3().UploadToWebsiteBucket(downloadsHtmlFile, "index.html", GetDownloadsWebsiteDirectory(countryCode));
        }

        private static bool UploadDownloadsIndexFile()
        {
            return new S3().UploadToWebsiteBucket(DownloadsIndexHtmlFile, "index.html", "downloads");
        }

        private static string GetDownloadsHtmlFile(CountryCode countryCode)
        {
            return Path.Combine(Config.LANDERIST_COM_OUTPUT!, GetDownloadsWebsiteDirectory(countryCode), "index.html");
        }

        private static string GetDownloadsWebsiteDirectory(CountryCode countryCode)
        {
            return $"downloads/{countryCode.ToString().ToLowerInvariant()}";
        }

        private static string FormatBytes(long bytes)
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
