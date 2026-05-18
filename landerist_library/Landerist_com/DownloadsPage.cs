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
        private const string LISTINGS_BY_OPERATION_PROPERTY_TYPE_SUBDIRECTORY = "ES/OperationPropertyTypes";

        private static readonly string DownloadsTemplateHtmlFile =
            Path.Combine(Config.LANDERIST_COM_TEMPLATES!, "downloads_template.html");

        private static readonly string DownloadsHtmlFile =
            Path.Combine(Config.LANDERIST_COM_OUTPUT!, "downloads.html");

        private static string DownloadsTemplate = string.Empty;

        public static void Update()
        {
            try
            {
                DownloadsTemplate = File.ReadAllText(DownloadsTemplateHtmlFile);
                UpdateUpdatedAt();

                foreach (ExportType exportType in GetDownloadsExportTypes())
                {
                    UpdateDownloadsTemplate(CountryCode.ES, exportType);
                }

                UpdateListingsByOperationPropertyTypeTemplate();
                UpdateHostsTemplate();

                if (UploadDownloadsFile())
                {
                    Log.WriteInfo("DownloadsPage", "Updated downloads page");
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("DownloadsPage Update", exception);
            }
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

        private static void UpdateHostsTemplate()
        {
            Replace("/*HOSTS*/", GetHostsTableRows());
        }

        private static void UpdateListingsByOperationPropertyTypeTemplate()
        {
            Replace("/*LISTINGS_BY_OPERATION_PROPERTY_TYPE*/", GetListingsByOperationPropertyTypeTableRows());
        }

        private static string GetListingsByOperationPropertyTypeTableRows()
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
                        operation,
                        propertyType,
                        publishedListingsCount,
                        unpublishedListingsCount));
                }
            }

            return rows.ToString();
        }

        private static string GetListingsByOperationPropertyTypeTableRow(
            Operation operation,
            PropertyType propertyType,
            int publishedListingsCount,
            int unpublishedListingsCount)
        {
            string label = $"{operation} {propertyType}";

            return
                "                <tr>" + Environment.NewLine +
                $"                    <td>{WebUtility.HtmlEncode(label)}</td>" + Environment.NewLine +
                $"                    <td>{GetListingsByOperationPropertyTypeDownloadCellText(operation, propertyType, ListingStatus.published, publishedListingsCount)}</td>" + Environment.NewLine +
                $"                    <td>{GetListingsByOperationPropertyTypeDownloadCellText(operation, propertyType, ListingStatus.unpublished, unpublishedListingsCount)}</td>" + Environment.NewLine +
                "                </tr>";
        }

        private static string GetListingsByOperationPropertyTypeDownloadCellText(
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

            string? url = GetListingsByOperationPropertyTypeDownloadUrl(operation, propertyType, listingStatus);
            if (string.IsNullOrWhiteSpace(url))
            {
                return counterText;
            }

            string fileName = GetListingsByOperationPropertyTypeFileName(CountryCode.ES, operation, propertyType, listingStatus, "json");
            return $"<a title=\"Download\" href=\"{WebUtility.HtmlEncode(url)}\" download=\"{WebUtility.HtmlEncode(fileName)}\">{counterText}</a>";
        }

        private static string? GetListingsByOperationPropertyTypeDownloadUrl(
            Operation operation,
            PropertyType propertyType,
            ListingStatus listingStatus)
        {
            string objectKey = GetListingsByOperationPropertyTypeObjectKey(operation, propertyType, listingStatus, "json");
            var (lastModified, contentLength) = new S3().GetFileInfo(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey);

            if (lastModified is null || contentLength is null)
            {
                return null;
            }

            return $"https://{PrivateConfig.AWS_S3_DOWNLOADS_BUCKET}.s3.amazonaws.com/{objectKey}";
        }

        private static string GetListingsByOperationPropertyTypeObjectKey(
            Operation operation,
            PropertyType propertyType,
            ListingStatus listingStatus,
            string extension)
        {
            return $"{LISTINGS_BY_OPERATION_PROPERTY_TYPE_SUBDIRECTORY}/{GetListingsByOperationPropertyTypeFileName(CountryCode.ES, operation, propertyType, listingStatus, extension)}";
        }

        private static string GetHostsTableRows()
        {
            StringBuilder rows = new();

            foreach (var website in Websites.Websites.GetApplySpecialRules()
                .OrderBy(website => website.Host, StringComparer.OrdinalIgnoreCase))
            {
                int publishedListingsCount = website.GetNumPublishedListings();
                int unpublishedListingsCount = website.GetNumUnpublishedListings();

                rows.AppendLine(GetHostTableRow(
                    website,
                    publishedListingsCount,
                    unpublishedListingsCount));
            }

            return rows.ToString();
        }

        private static string GetHostTableRow(
            Website website,
            int publishedListingsCount,
            int unpublishedListingsCount)
        {
            return
                "                <tr>" + Environment.NewLine +
                $"                    <td>{WebUtility.HtmlEncode(website.Host)}</td>" + Environment.NewLine +
                $"                    <td>{GetHostDownloadCellText(website.Host, ListingStatus.published, "json", publishedListingsCount)}</td>" + Environment.NewLine +
                $"                    <td>{GetHostDownloadCellText(website.Host, ListingStatus.unpublished, "json", unpublishedListingsCount)}</td>" + Environment.NewLine +
                "                </tr>";
        }

        private static string GetHostDownloadCellText(string host, ListingStatus listingStatus, string extension, int counter)
        {
            string counterText = counter.ToString(CultureInfo.InvariantCulture);
            if (counter <= 0)
            {
                return counterText;
            }

            string? url = GetHostDownloadUrl(host, listingStatus, extension);
            if (string.IsNullOrWhiteSpace(url))
            {
                return counterText;
            }

            string fileName = GetHostListingsFileName(CountryCode.ES, host, listingStatus, extension);
            return $"<a title=\"Download\" href=\"{WebUtility.HtmlEncode(url)}\" download=\"{WebUtility.HtmlEncode(fileName)}\">{counterText}</a>";
        }

        private static string? GetHostDownloadUrl(string host, ListingStatus listingStatus, string extension)
        {
            string objectKey = GetHostObjectKey(host, listingStatus, extension);
            var (lastModified, contentLength) = new S3().GetFileInfo(PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, objectKey);

            if (lastModified is null || contentLength is null)
            {
                return null;
            }

            return $"https://{PrivateConfig.AWS_S3_DOWNLOADS_BUCKET}.s3.amazonaws.com/{objectKey}";
        }

        private static string GetHostObjectKey(string host, ListingStatus listingStatus, string extension)
        {
            return $"ES/Hosts/{GetHostListingsFileName(CountryCode.ES, host, listingStatus, extension)}";
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

        private static bool UploadDownloadsFile()
        {
            File.WriteAllText(DownloadsHtmlFile, DownloadsTemplate);
            return new S3().UploadToWebsiteBucket(DownloadsHtmlFile, "index.html", "downloads");
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
