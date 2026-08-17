using landerist_library.Application.Websites;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Infrastructure.WebsiteServices;
using System.Globalization;
using System.Net;
using System.Text;
using landerist_library.Infrastructure.Runtime;

namespace landerist_library.Infrastructure.Distribution
{
    public sealed class HostsStatisticsPage : DistributionArtifacts
    {
        private readonly WebsiteMetricsService _websiteMetrics;
        private readonly IWebsiteCatalog _websites;
        private readonly string HostsStatisticsTemplateHtmlFile;
        private readonly string HostsStatisticsHtmlFile;

        public HostsStatisticsPage(
            WebsiteMetricsService websiteMetrics,
            IWebsiteCatalog websites,
            DistributionOptions options) : base(options)
        {
            ArgumentNullException.ThrowIfNull(websiteMetrics);
            ArgumentNullException.ThrowIfNull(websites);
            _websiteMetrics = websiteMetrics;
            _websites = websites;
            HostsStatisticsTemplateHtmlFile = Path.Combine(options.TemplatesDirectory, "hosts-statistics", "hosts_statistics_template.html");
            HostsStatisticsHtmlFile = Path.Combine(options.OutputDirectory, "hosts_statistics.html");
        }

        private readonly CultureInfo SummaryCulture = CultureInfo.GetCultureInfo("es-ES");

        private const int RecentListingsDays = 7;
        private const int RecentScrapedPagesDays = 3;
        private const int RecentInsertedPagesDays = 3;
        private const int RecentParseListingPagesDays = 3;
        private const int StaleWebsiteDateAlertDays = 3;
        private const decimal LowScrapedPagesAlertThreshold = 0.01m;

        public void Update()
        {
            try
            {
                var template = File.ReadAllText(HostsStatisticsTemplateHtmlFile);
                var websites = _websites.GetAll()
                    .OrderBy(website => website.Host, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                template = template.Replace("/*UPDATED_AT*/", DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
                template = template.Replace("/*HOSTS_STATISTICS_ROWS*/", GetHostsStatisticsRows(websites));

                File.WriteAllText(HostsStatisticsHtmlFile, template);

                if (new S3().UploadToWebsiteBucket(HostsStatisticsHtmlFile, "index.html", "hosts-statistics"))
                {
                    Log.WriteInfo("HostsStatisticsPage", "Updated hosts statistics page");
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("HostsStatisticsPage Update", exception);
            }
        }

        private string GetHostsStatisticsRows(IEnumerable<Websites.Website> websites)
        {
            StringBuilder rows = new();

            foreach (var website in websites)
            {
                rows.AppendLine(GetHostsStatisticsRow(website));
            }

            return rows.ToString();
        }

        private string GetHostsStatisticsRow(Websites.Website website)
        {
            int totalPages = _websiteMetrics.CountPages(website);
            int recentScrapedPages = _websiteMetrics.CountPagesScrapedSince(website, DateTime.Now.AddDays(-RecentScrapedPagesDays));
            int recentInsertedPages = _websiteMetrics.CountPagesInsertedSince(website, DateTime.Now.AddDays(-RecentInsertedPagesDays));
            int recentParseListingPages = _websiteMetrics.CountPagesParsedSince(website, DateTime.Now.AddDays(-RecentParseListingPagesDays));
            int totalListings = _websiteMetrics.CountListings(website);
            int recentListings = _websiteMetrics.CountListingsSince(website, DateTime.Now.AddDays(-RecentListingsDays));
            int publishedListings = _websiteMetrics.CountPublishedListings(website);
            int unpublishedListings = _websiteMetrics.CountUnpublishedListings(website);

            return
                "                        <tr>" + Environment.NewLine +
                $"                            {FormatTableCell(FormatHostLink(website.Host), website.Host)}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatWebsiteDate(website.RobotsTxtUpdated), FormatDateSortValue(website.RobotsTxtUpdated), IsStaleWebsiteDate(website.RobotsTxtUpdated))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatWebsiteDate(website.SitemapUpdated), FormatDateSortValue(website.SitemapUpdated), IsStaleWebsiteDate(website.SitemapUpdated))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatNumber(totalPages), FormatNumberSortValue(totalPages))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatPercentage(recentScrapedPages, totalPages), FormatPercentageSortValue(recentScrapedPages, totalPages), IsLowScrapedPagesPercentage(recentScrapedPages, totalPages), FormatPercentageTitle(recentScrapedPages, totalPages))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatPercentage(recentInsertedPages, totalPages), FormatPercentageSortValue(recentInsertedPages, totalPages), title: FormatPercentageTitle(recentInsertedPages, totalPages))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatPercentage(recentParseListingPages, totalPages), FormatPercentageSortValue(recentParseListingPages, totalPages), IsFullPercentage(recentParseListingPages, totalPages), FormatPercentageTitle(recentParseListingPages, totalPages))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatNumber(totalListings), FormatNumberSortValue(totalListings))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatPercentage(recentListings, totalListings), FormatPercentageSortValue(recentListings, totalListings), title: FormatPercentageTitle(recentListings, totalListings))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatPercentage(publishedListings, totalListings), FormatPercentageSortValue(publishedListings, totalListings), IsZeroOrFullPercentage(publishedListings, totalListings), FormatPercentageTitle(publishedListings, totalListings))}" + Environment.NewLine +
                $"                            {FormatTableCell(FormatPercentage(unpublishedListings, totalListings), FormatPercentageSortValue(unpublishedListings, totalListings), IsZeroOrFullPercentage(unpublishedListings, totalListings), FormatPercentageTitle(unpublishedListings, totalListings))}" + Environment.NewLine +
                "                        </tr>";
        }

        private string FormatTableCell(string html, string sortValue, bool alert = false, string? title = null)
        {
            if (alert)
            {
                html = $"<span class=\"stat-alert\">{html}</span>";
            }

            string titleAttribute = title == null
                ? string.Empty
                : $" title=\"{WebUtility.HtmlEncode(title)}\"";

            return $"<td data-sort=\"{WebUtility.HtmlEncode(sortValue)}\"{titleAttribute}>{html}</td>";
        }

        private bool IsStaleWebsiteDate(DateTime? dateTime)
        {
            return dateTime < DateTime.Now.AddDays(-StaleWebsiteDateAlertDays);
        }

        private bool IsLowScrapedPagesPercentage(int value, int total)
        {
            return total > 0 && (decimal)value / total <= LowScrapedPagesAlertThreshold;
        }

        private bool IsFullPercentage(int value, int total)
        {
            return total > 0 && value >= total;
        }

        private bool IsZeroOrFullPercentage(int value, int total)
        {
            return total > 0 && (value == 0 || value >= total);
        }

        private string FormatDateSortValue(DateTime? dateTime)
        {
            return dateTime?.ToString("yyyyMMdd", CultureInfo.InvariantCulture) ?? "0";
        }

        private string FormatNumberSortValue(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private string FormatPercentageSortValue(int value, int total)
        {
            if (total <= 0)
            {
                return decimal.Zero.ToString(CultureInfo.InvariantCulture);
            }

            return ((decimal)value / total).ToString(CultureInfo.InvariantCulture);
        }

        private string FormatPercentageTitle(int value, int total)
        {
            return $"{FormatNumber(value)} / {FormatNumber(total)}";
        }

        private string FormatHostLink(string host)
        {
            string href = "/host-statistics/#" + Uri.EscapeDataString(host);
            return $"<a href=\"{WebUtility.HtmlEncode(href)}\">{WebUtility.HtmlEncode(host)}</a>";
        }

        private string FormatWebsiteDate(DateTime? dateTime)
        {
            return dateTime?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private string FormatNumber(int value)
        {
            return value.ToString("N0", SummaryCulture);
        }

        private string FormatPercentage(int value, int total)
        {
            if (total <= 0)
            {
                return decimal.Zero.ToString("P1", SummaryCulture);
            }

            return ((decimal)value / total).ToString("P1", SummaryCulture);
        }
    }
}
