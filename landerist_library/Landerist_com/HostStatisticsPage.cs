using landerist_library.Configuration;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Statistics;
using System.Data;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace landerist_library.Landerist_com
{
    public class HostStatisticsPage : Landerist_com
    {
        private static readonly string HostStatisticsTemplateHtmlFile =
            Path.Combine(Config.LANDERIST_COM_TEMPLATES!, "host_statistics_template.html");

        private static readonly string HostStatisticsHtmlFile =
            Path.Combine(Config.LANDERIST_COM_OUTPUT!, "host_statistics.html");

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static void Update()
        {
            try
            {
                var template = File.ReadAllText(HostStatisticsTemplateHtmlFile);
                var websites = Websites.Websites.GetApplySpecialRules()
                    .OrderBy(website => website.Host, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                template = template.Replace("/*UPDATED_AT*/", DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
                template = template.Replace("/*HOST_OPTIONS*/", GetHostOptions(websites));
                template = template.Replace("/*HOST_STATISTICS_DATA*/", JsonSerializer.Serialize(GetHostStatistics(websites), JsonSerializerOptions));

                File.WriteAllText(HostStatisticsHtmlFile, template);

                if (new S3().UploadToWebsiteBucket(HostStatisticsHtmlFile, "index.html", "host-statistics"))
                {
                    Log.WriteInfo("HostStatisticsPage", "Updated host statistics page");
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("HostStatisticsPage Update", exception);
            }
        }

        private static string GetHostOptions(IEnumerable<Websites.Website> websites)
        {
            return string.Join(
                Environment.NewLine,
                websites.Select(website =>
                    $"                        <option value=\"{WebUtility.HtmlEncode(website.Host)}\">{WebUtility.HtmlEncode(website.Host)}</option>"));
        }

        private static Dictionary<string, HostStatisticsModel> GetHostStatistics(IEnumerable<Websites.Website> websites)
        {
            Dictionary<string, HostStatisticsModel> dictionary = new(StringComparer.OrdinalIgnoreCase);

            foreach (var website in websites)
            {
                dictionary[website.Host] = GetHostStatistics(website);
            }

            return dictionary;
        }

        private static HostStatisticsModel GetHostStatistics(Websites.Website website)
        {
            return new HostStatisticsModel
            {
                MainUri = website.MainUri.AbsoluteUri,
                UpdatedAt = HostStatistics.GetLatestDate(website.Host)?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Summary = new HostStatisticsSummary
                {
                    TotalPages = website.GetNumPages(),
                    TotalListings = website.GetNumListings(),
                    PublishedListings = website.GetNumPublishedListings(),
                    PublishedListingsWithAddress = website.GetNumPublishedListingsWithAddress(),
                    PublishedListingsWithCoordinates = website.GetNumPublishedListingsWithCoordinates(),
                    PublishedListingsWithImages = website.GetNumPublishedListingsWithImages(),
                    UnpublishedListings = website.GetNumUnpublishedListings()
                },
                Charts = new HostStatisticsCharts
                {
                    Inserted = GetTimeSeries(website.Host, HostStatisticsKey.Inserted, "Inserted Pages"),
                    Updated = GetTimeSeries(website.Host, HostStatisticsKey.Updated, "Updated Pages"),
                    NotListingCache = GetTimeSeries(website.Host, HostStatisticsKey.NotListingCache, "Not listing by cache"),
                    ResponseBodyTextAlreadyParsed = GetTimeSeries(website.Host, HostStatisticsKey.ResponseBodyTextAlreadyParsed, "ResponseBodyText already parsed"),
                    ReponseBodyTextIsAnotherListingInHost = GetTimeSeries(website.Host, HostStatisticsKey.ReponseBodyTextIsAnotherListingInHost, "ResponseBodyText is another listing in host"),
                    NextUpdateDistribution = GetDistribution(HostStatistics.GetPagesByNextUpdate(website.Host), "nextupdate"),
                    PageType = GetDistribution(HostStatistics.GetPagesByPageType(website.Host), "PageType"),
                    HttpStatusCode = GetDistribution(HostStatistics.GetPagesByHttpStatusCode(website.Host), "HttpStatusCode"),
                }
            };
        }

        private static List<ChartSeriesModel> GetTimeSeries(string host, HostStatisticsKey key, string label)
        {
            var dataTable = HostStatistics.GetLatestStatistics(host, key.ToString(), 15);
            List<ChartPointModel> values = [];

            foreach (DataRow dataRow in dataTable.Rows.Cast<DataRow>().Reverse())
            {
                values.Add(new ChartPointModel
                {
                    Key = ((DateTime)dataRow["Date"]).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Value = Convert.ToInt32(dataRow["Counter"])
                });
            }

            return values.Count == 0
                ? []
                : [new ChartSeriesModel { Label = label, Values = values }];
        }

        private static List<ChartSeriesModel> GetLatestDistribution(string host, HostStatisticsKey keyPrefix, string label)
        {
            var dataTable = HostStatistics.GetLatestStatisticsByPrefix(host, keyPrefix.ToString());
            List<ChartPointModel> values = [];

            foreach (DataRow dataRow in dataTable.Rows)
            {
                values.Add(new ChartPointModel
                {
                    Key = RemovePrefix((string)dataRow["Key"], keyPrefix),
                    Value = Convert.ToInt32(dataRow["Counter"])
                });
            }

            return values.Count == 0
                ? []
                : [new ChartSeriesModel { Label = label, Values = values }];
        }

        private static List<ChartSeriesModel> GetDistribution(DataTable dataTable, string label)
        {
            List<ChartPointModel> values = [];

            foreach (DataRow dataRow in dataTable.Rows)
            {
                values.Add(new ChartPointModel
                {
                    Key = (string)dataRow["Key"],
                    Value = Convert.ToInt32(dataRow["Counter"])
                });
            }

            return values.Count == 0
                ? []
                : [new ChartSeriesModel { Label = label, Values = values }];
        }

        private static List<ChartSeriesModel> GetHistoricalDistribution(string host, HostStatisticsKey keyPrefix)
        {
            List<ChartSeriesModel> series = [];

            foreach (var key in HostStatistics.GetKeysLike(host, keyPrefix))
            {
                var values = GetTimeSeries(host, key, RemovePrefix(key, keyPrefix));
                if (values.Count > 0)
                {
                    series.AddRange(values);
                }
            }

            return series;
        }

        private static List<ChartSeriesModel> GetTimeSeries(string host, string key, string label)
        {
            var dataTable = HostStatistics.GetLatestStatistics(host, key, 15);
            List<ChartPointModel> values = [];

            foreach (DataRow dataRow in dataTable.Rows.Cast<DataRow>().Reverse())
            {
                values.Add(new ChartPointModel
                {
                    Key = ((DateTime)dataRow["Date"]).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Value = Convert.ToInt32(dataRow["Counter"])
                });
            }

            return values.Count == 0
                ? []
                : [new ChartSeriesModel { Label = label, Values = values }];
        }

        private static string RemovePrefix(string key, HostStatisticsKey keyPrefix)
        {
            string prefix = keyPrefix + "_";
            return key.StartsWith(prefix, StringComparison.Ordinal)
                ? key[prefix.Length..]
                : key;
        }

        private sealed class HostStatisticsModel
        {
            public required string MainUri { get; init; }
            public string? UpdatedAt { get; init; }
            public required HostStatisticsSummary Summary { get; init; }
            public required HostStatisticsCharts Charts { get; init; }
        }

        private sealed class HostStatisticsSummary
        {
            public required int TotalPages { get; init; }
            public required int TotalListings { get; init; }
            public required int PublishedListings { get; init; }
            public required int PublishedListingsWithAddress { get; init; }
            public required int PublishedListingsWithCoordinates { get; init; }
            public required int PublishedListingsWithImages { get; init; }
            public required int UnpublishedListings { get; init; }
        }

        private sealed class HostStatisticsCharts
        {
            public required List<ChartSeriesModel> Inserted { get; init; }
            public required List<ChartSeriesModel> Updated { get; init; }
            public required List<ChartSeriesModel> NotListingCache { get; init; }
            public required List<ChartSeriesModel> ResponseBodyTextAlreadyParsed { get; init; }
            public required List<ChartSeriesModel> ReponseBodyTextIsAnotherListingInHost { get; init; }
            public required List<ChartSeriesModel> NextUpdateDistribution { get; init; }
            public required List<ChartSeriesModel> PageType { get; init; }
            public required List<ChartSeriesModel> HttpStatusCode { get; init; }
        }

        private sealed class ChartSeriesModel
        {
            public required string Label { get; init; }
            public required List<ChartPointModel> Values { get; init; }
        }

        private sealed class ChartPointModel
        {
            public required string Key { get; init; }
            public required int Value { get; init; }
        }
    }
}
