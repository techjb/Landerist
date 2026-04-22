using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Downloaders;
using landerist_library.Index;
using landerist_library.Tools;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Data;
using System.IO.Compression;
using System.Text;

namespace landerist_library.Pages
{

    public class Page : IDisposable
    {
        public string Host { get; set; } = string.Empty;

        public Uri Uri { get; set; } = new Uri("about:blank");

        public string UriHash { get; set; } = string.Empty;

        public DateTime Inserted { get; set; }

        public DateTime? Updated { get; set; }

        public DateTime? NextUpdate { get; set; }

        public short? HttpStatusCode { get; set; }

        public string? RedirectUrl { get; set; } = null;

        public string? Etag { get; set; } = null;

        public PageType? PageType { get; private set; }

        public short? PageTypeCounter { get; private set; }

        private ListingStatus? ListingStatus { get; set; }

        public string? LockedBy { get; set; }

        public WaitingStatus? WaitingStatus { get; private set; }

        private string? ResponseBody { get; set; }

        public string? ResponseBodyText { get; set; }

        public string? ResponseBodyTextHash { get; set; }

        public short? ResponseBodyTextNotChangedCounter { get; private set; }

        public short? TransientErrorCounter { get; private set; }

        public byte[]? ResponseBodyZipped { get; private set; }

        public int? TokenCount { get; set; } = null;

        public bool ResponseBodyTextNotChanged { get; set; } = false;

        public byte[]? Screenshot { get; set; }

        private bool EtagNotChanged { get; set; } = false;

        private bool HasComparableEtag { get; set; } = false;


        private HtmlDocument? HtmlDocument = null;


        private string? OriginalOuterHtml = null;

        private string? ParseListingUserInput = null;


        public Website Website = new();


        private bool Disposed;

        public Page(string url) : this(new Uri(url))
        {

        }

        public Page(Uri uri) : this(Websites.Websites.GetWebsite(uri.Host), uri)
        {

        }

        public Page(Website website) : this(website, website.MainUri)
        {

        }

        public Page(Website website, Uri uri)
        {
            Website = website;
            Host = uri.Host;
            Uri = uri;
            UriHash = GetUriHash();
            Inserted = DateTime.Now;
            Updated = DateTime.Now;

            var dataRow = GetDataRow();
            if (dataRow != null)
            {
                Load(dataRow);
            }
        }

        public string GetUriHash()
        {
            var uriString = Uri.ToString();
            return Strings.GetHash(uriString);
        }

        public Page(Website website, DataRow dataRow)
        {
            Website = website;
            Load(dataRow);
        }

        private void Load(DataRow dataRow)
        {
            Host = dataRow["Host"].ToString()!;
            string uriString = dataRow["Uri"].ToString()!;
            Uri = new Uri(uriString);
            UriHash = dataRow["UriHash"].ToString()!;
            Inserted = (DateTime)dataRow["Inserted"];
            Updated = dataRow["Updated"] is DBNull ? null : (DateTime)dataRow["Updated"];
            NextUpdate = dataRow["NextUpdate"] is DBNull ? null : (DateTime)dataRow["NextUpdate"];
            HttpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
            Etag = dataRow.Table.Columns.Contains("Etag") && dataRow["Etag"] is not DBNull
                ? dataRow["Etag"].ToString()
                : null;
            PageType = dataRow["PageType"] is DBNull ? null : (PageType)Enum.Parse(typeof(PageType), dataRow["PageType"].ToString()!);
            PageTypeCounter = dataRow["PageTypeCounter"] is DBNull ? null : (short)dataRow["PageTypeCounter"];
            ListingStatus = dataRow["ListingStatus"] is DBNull ? null : (ListingStatus)Enum.Parse(typeof(ListingStatus), dataRow["ListingStatus"].ToString()!);
            LockedBy = dataRow["LockedBy"] is DBNull ? null : dataRow["LockedBy"].ToString();
            WaitingStatus = dataRow["WaitingStatus"] is DBNull ? null : (WaitingStatus)Enum.Parse(typeof(WaitingStatus), dataRow["WaitingStatus"].ToString()!);
            ResponseBodyTextHash = dataRow["ResponseBodyTextHash"] is DBNull ? null : dataRow["ResponseBodyTextHash"].ToString();
            ResponseBodyTextNotChangedCounter = dataRow.Table.Columns.Contains("ResponseBodyTextNotChangedCounter") && dataRow["ResponseBodyTextNotChangedCounter"] is not DBNull
                ? (short)dataRow["ResponseBodyTextNotChangedCounter"]
                : null;
            TransientErrorCounter = dataRow.Table.Columns.Contains("TransientErrorCounter") && dataRow["TransientErrorCounter"] is not DBNull
                ? (short)dataRow["TransientErrorCounter"]
                : null;
            ResponseBodyZipped = dataRow["ResponseBodyZipped"] is DBNull ? null : (byte[])dataRow["ResponseBodyZipped"];
            TokenCount = dataRow["TokenCount"] is DBNull ? null : (int?)dataRow["TokenCount"];
        }

        public DataRow? GetDataRow()
        {
            string query =
                "SELECT * " +
                "FROM " + Pages.PAGES + " " +
                "WHERE [UriHash] = @UriHash";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"UriHash", UriHash }
            });

            if (dataTable.Rows.Count > 0)
            {
                return dataTable.Rows[0];
            }
            return null;
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + Pages.PAGES + " (" +
                "[Host], [Uri], [UriHash], [Inserted], [Updated], [NextUpdate], [HttpStatusCode], [Etag], [PageType], " +
                "[PageTypeCounter], [ListingStatus], [LockedBy], [WaitingStatus], [ResponseBodyTextHash], " +
                "[ResponseBodyTextNotChangedCounter], [TransientErrorCounter], [ResponseBodyZipped], [TokenCount]) " +
                "VALUES(@Host, @Uri, @UriHash, @Inserted, @Updated, @NextUpdate, @HttpStatusCode, @Etag, @PageType, " +
                "@PageTypeCounter, @ListingStatus, @LockedBy, @WaitingStatus, @ResponseBodyTextHash, " +
                "@ResponseBodyTextNotChangedCounter, @TransientErrorCounter, CONVERT(varbinary(max), @ResponseBodyZipped), @TokenCount)";

            bool sucess = new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", Host },
                {"Uri", Uri.ToString() },
                {"UriHash", UriHash },
                {"Inserted", DateTime.Now },
                {"Updated", null },
                {"NextUpdate", null },
                {"HttpStatusCode", null },
                {"Etag", null },
                {"PageType", null },
                {"PageTypeCounter", null },
                {"ListingStatus", null },
                {"LockedBy", null },
                {"WaitingStatus", null },
                {"ResponseBodyTextHash", null },
                {"ResponseBodyTextNotChangedCounter", null },
                {"TransientErrorCounter", null },
                {"ResponseBodyZipped", null  },
                {"TokenCount", null  },
            });
            return sucess;
        }


        public bool SetPageTypeAndNextUpdate(PageType? pageType)
        {
            SetPageType(pageType);
            SetNextUpdate();
            return Update();
        }

        public bool Update()
        {
            //if (Config.IsConfigurationLocal())
            //{
            //    return true;
            //}

            Updated = DateTime.Now;

            string query =
                "UPDATE " + Pages.PAGES + " SET " +
                "[Updated] = @Updated, " +
                "[NextUpdate] = @NextUpdate, " +
                "[HttpStatusCode] = @HttpStatusCode, " +
                "[Etag] = @Etag, " +
                "[PageType] = @PageType, " +
                "[PageTypeCounter] = @PageTypeCounter, " +
                "[ListingStatus] = @ListingStatus, " +
                "[LockedBy] = @LockedBy, " +
                "[WaitingStatus] = @WaitingStatus, " +
                "[ResponseBodyTextHash] = @ResponseBodyTextHash, " +
                "[ResponseBodyTextNotChangedCounter] = @ResponseBodyTextNotChangedCounter, " +
                "[TransientErrorCounter] = @TransientErrorCounter, " +
                "[ResponseBodyZipped] = CASE WHEN @ResponseBodyZipped IS NULL THEN NULL ELSE CONVERT(varbinary(max), @ResponseBodyZipped) END," +
                "[TokenCount] = @TokenCount " +
                "WHERE [UriHash] = @UriHash";

            var sucess = new DataBase().Query(query, new Dictionary<string, object?> {
                {"Updated", Updated },
                {"NextUpdate", NextUpdate },
                {"HttpStatusCode", HttpStatusCode},
                {"Etag", Etag},
                {"PageType", PageType?.ToString()},
                {"PageTypeCounter", PageTypeCounter},
                {"ListingStatus", ListingStatus?.ToString()},
                {"LockedBy", LockedBy?.ToString()},
                {"WaitingStatus", WaitingStatus?.ToString()},
                {"ResponseBodyTextHash", ResponseBodyTextHash},
                {"ResponseBodyTextNotChangedCounter", ResponseBodyTextNotChangedCounter},
                {"TransientErrorCounter", TransientErrorCounter},
                {"ResponseBodyZipped", ResponseBodyZipped},
                {"TokenCount", TokenCount},
                {"UriHash", UriHash },
            });

            if (!sucess)
            {
                Logs.Log.WriteError("Page Update", "Failed to update page: " + Uri);
            }
            return sucess;
        }

        public void SetNextUpdate()
        {
            NextUpdate = PageNextUpdateCalculator.Calculate(this, DateTime.Now);
        }

        public bool UpdateNextUpdate()
        {
            string query =
               "UPDATE " + Pages.PAGES + " SET " +
               "[NextUpdate] = @NextUpdate " +
               "WHERE [UriHash] = @UriHash";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", UriHash },
                {"NextUpdate", NextUpdate },
            });
        }

        public bool Delete()
        {
            string query =
                "DELETE FROM " + Pages.PAGES + " " +
                "WHERE [UriHash] = @UriHash";

            bool sucess = new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", UriHash }
            });
            return sucess &&
                ES_Listings.Delete(UriHash) &&
                ES_Media.Delete(UriHash) &&
                ES_Sources.Delete(UriHash);
        }

        public bool DeleteListing()
        {
            var listing = ES_Listings.GetListing(this, false, false);
            if (listing != null)
            {
                if (ES_Listings.Delete(listing))
                {
                    ES_Media.Delete(listing);
                    ES_Sources.Delete(listing);
                    return true;
                }
            }
            return false;
        }

        public HtmlDocument? GetHtmlDocument()
        {

            if (HtmlDocument != null && OriginalOuterHtml != null &&
                OriginalOuterHtml.Equals(HtmlDocument.DocumentNode.OuterHtml))
            {
                return HtmlDocument;
            }
            if (!string.IsNullOrEmpty(ResponseBody))
            {
                HtmlDocument = null;
                OriginalOuterHtml = null;

                try
                {
                    HtmlDocument = new();
                    HtmlDocument.LoadHtml(ResponseBody);
                    OriginalOuterHtml = HtmlDocument.DocumentNode.OuterHtml;
                    return HtmlDocument;
                }
                catch (Exception exception)
                {
                    Logs.Log.WriteError("Page GetHtmlDocument", Uri, exception);
                }
            }
            return null;
        }

        public bool IsMainPage()
        {
            if (Website == null)
            {
                return false;
            }
            return Uri.Equals(Website.MainUri);
        }

        public bool ResponseBodyIsNullOrEmpty()
        {
            return string.IsNullOrEmpty(ResponseBody);
        }

        public void SetDownloadedData(IDownloader downloader)
        {
            var previousEtag = NormalizeEtag(Etag);
            var downloadedEtag = NormalizeEtag(downloader.Etag);

            HasComparableEtag = !string.IsNullOrEmpty(previousEtag) && !string.IsNullOrEmpty(downloadedEtag);
            EtagNotChanged = HasComparableEtag && string.Equals(previousEtag, downloadedEtag, StringComparison.Ordinal);

            ResponseBody = downloader.Content;
            ResponseBodyText = null;
            Screenshot = downloader.Screenshot;
            HttpStatusCode = downloader.HttpStatusCode;
            RedirectUrl = downloader.RedirectUrl;
            Etag = downloadedEtag;
        }

        public void SetResponseBodyText()
        {
            var htmlDocument = GetHtmlDocument();
            if (htmlDocument == null)
            {
                ResponseBodyTextNotChanged = false;
                ResponseBodyTextNotChangedCounter = null;
                return;
            }

            ResponseBodyText = HtmlToText.GetText(htmlDocument);
            if (string.IsNullOrEmpty(ResponseBodyText))
            {
                ResponseBodyTextNotChanged = false;
                ResponseBodyTextNotChangedCounter = null;
                return;
            }

            string hash = Strings.GetHash(ResponseBodyText);
            ResponseBodyTextNotChanged = hash == ResponseBodyTextHash;
            ResponseBodyTextNotChangedCounter = ResponseBodyTextNotChanged
                ? (short)Math.Min((ResponseBodyTextNotChangedCounter ?? 0) + 1, Config.MAX_PAGETYPE_COUNTER)
                : (short)0;
            ResponseBodyTextHash = hash;
        }
        public bool EtagHasNotChanged()
        {
            if (HasComparableEtag)
            {
                return EtagNotChanged;
            }
            return false;
        }


        public bool ResponseBodyTextHasNotChanged()
        {
            return ResponseBodyTextNotChanged && (IsListing() || IsNotListingByParser());
        }


        private static string? NormalizeEtag(string? etag)
        {
            return string.IsNullOrWhiteSpace(etag) ? null : etag.Trim();
        }

        public bool ResponseBodyTextIsError()
        {
            if (ResponseBodyText == null)
            {
                return false;
            }
            return
                ResponseBodyText.StartsWith("Not found", StringComparison.OrdinalIgnoreCase) ||
                ResponseBodyText.StartsWith("Error", StringComparison.OrdinalIgnoreCase) ||
                ResponseBodyText.StartsWith("404", StringComparison.OrdinalIgnoreCase) ||
                ResponseBodyText.Contains("algo salió mal", StringComparison.OrdinalIgnoreCase) ||
                ResponseBodyText.Contains("Page Not found", StringComparison.OrdinalIgnoreCase)
                ;
        }

        public bool ResponseBodyTextIsTooLarge()
        {
            if (ResponseBodyText is null)
            {
                return false;
            }
            return ResponseBodyText.Length > Config.MAX_RESPONSEBODYTEXT_LENGTH;
        }

        public bool ResponseBodyTextIsTooShort()
        {
            if (string.IsNullOrEmpty(ResponseBodyText))
            {
                return true;
            }
            return ResponseBodyText.Length < Config.MIN_RESPONSEBODYTEXT_LENGTH;
        }

        public bool ReponseBodyTextIsAnotherListingInHost()
        {
            if (string.IsNullOrEmpty(ResponseBodyText))
            {
                return false;
            }

            string query =
                "SELECT 1 " +
                "FROM " + Pages.PAGES + " " +
                "WHERE [HOST] = @Host AND " +
                "[UriHash] <> @UriHash AND " +
                "[ResponseBodyTextHash] = @ResponseBodyTextHash AND " +
                "[ListingStatus] IS NOT NULL";

            return new DataBase().QueryExists(query, new Dictionary<string, object?> {
                {"Host", Host},
                {"UriHash", UriHash },
                {"ResponseBodyTextHash", ResponseBodyTextHash },
            });
        }

        public bool IsNotListingCache()
        {
            if (string.IsNullOrEmpty(ResponseBodyTextHash))
            {
                return false;
            }
            return NotListingsCache.IsNotListing(ResponseBodyTextHash);
        }

        public bool InsertNotListingResponseBodyText()
        {
            return (ResponseBodyTextHash != null) && NotListingsCache.Insert(ResponseBodyTextHash);
        }

        public Listing? GetListing(bool loadMedia, bool loadSources)
        {
            return ES_Listings.GetListing(this, loadMedia, loadSources);
        }

        public bool ContainsListing()
        {
            var listing = GetListing(false, false);
            return listing is not null;
        }

        public bool ContainsMetaRobotsNoIndex()
        {
            return ContainsMetaRobots("noindex");
        }

        public bool ContainsMetaRobotsNoFollow()
        {
            return ContainsMetaRobots("nofollow");
        }

        public bool ContainsMetaRobotsNoImageIndex()
        {
            return ContainsMetaRobots("noimageindex");
        }

        public bool NotCanonical()
        {
            var canonicalUri = GetCanonicalUri();
            if (canonicalUri == null)
            {
                return false;
            }
            return !Uri.Equals(canonicalUri);
        }
        public bool RedirectToAnotherUrl()
        {
            return !string.IsNullOrEmpty(RedirectUrl);
        }

        public bool IncorrectLanguage()
        {
            var htmlDocument = GetHtmlDocument();
            if (htmlDocument != null)
            {
                var htmlNode = htmlDocument.DocumentNode.SelectSingleNode("/html");
                if (htmlNode != null)
                {
                    var lang = htmlNode.Attributes["lang"];
                    if (lang != null)
                    {
                        return !LanguageValidator.IsValidLanguageAndCountry(Website, lang.Value);
                    }
                }
            }
            return false;
        }

        public Uri? GetCanonicalUri()
        {
            var htmlDocument = GetHtmlDocument();
            if (htmlDocument != null)
            {
                var node = htmlDocument.DocumentNode.SelectSingleNode("//link[@rel='canonical']");
                if (node != null)
                {
                    var contentAttribute = node.GetAttributeValue("href", "");
                    return new Indexer(this).GetUri(contentAttribute);
                }
            }
            return null;
        }

        private bool ContainsMetaRobots(string content)
        {
            var htmlDocument = GetHtmlDocument();
            if (htmlDocument != null)
            {
                var node = htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='robots']");
                if (node != null)
                {
                    var contentAttribute = node.GetAttributeValue("content", "");
                    if (!string.IsNullOrEmpty(contentAttribute))
                    {
                        var contents = contentAttribute.Split(',');
                        foreach (var item in contents)
                        {
                            if (item.Equals(content) || item.Equals("none"))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public void SetPageType(PageType? newPageType)
        {
            SetTransientErrorCounter(newPageType);

            if (PageType == newPageType)
            {
                PageTypeCounter = (short)Math.Min((PageTypeCounter ?? 0) + 1, Config.MAX_PAGETYPE_COUNTER);
            }
            else
            {
                PageTypeCounter = 1;
                PageType = newPageType;
            }
        }

        private void SetTransientErrorCounter(PageType? newPageType)
        {
            if (newPageType == landerist_library.Pages.PageType.HttpStatusCodeNotOK ||
                newPageType == landerist_library.Pages.PageType.ResponseBodyNullOrEmpty)
            {
                TransientErrorCounter = (short)Math.Min((TransientErrorCounter ?? 0) + 1, Config.MAX_PAGETYPE_COUNTER);
                return;
            }

            TransientErrorCounter = 0;
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Host = string.Empty;
                UriHash = string.Empty;
                HtmlDocument = null;
                ResponseBody = null;
                ResponseBodyText = null;
                ResponseBodyTextHash = null;
                Etag = null;
                ResponseBodyZipped = null;
                Screenshot = null;
            }

            Disposed = true;
            GC.SuppressFinalize(this);
        }

        public bool ContainsScreenshot()
        {
            return Screenshot != null &&
                Screenshot.Length > 0 &&
                Screenshot.Length < Config.MAX_SCREENSHOT_SIZE;
        }

        public void SetWaitingStatusAIRequest()
        {
            SetWaitingStatus(landerist_library.Pages.WaitingStatus.waiting_ai_request);
        }

        private void SetWaitingStatus(WaitingStatus waitingStatus)
        {
            WaitingStatus = waitingStatus;
        }

        public void RemoveWaitingStatus()
        {
            WaitingStatus = null;
        }

        public void RemoveResponseBodyZipped()
        {
            ResponseBodyZipped = null;
        }

        public void RemoveResponseBody()
        {
            ResponseBody = null;
        }

        public bool SetResponseBodyZipped()
        {
            if (string.IsNullOrEmpty(ResponseBody))
            {
                ResponseBodyZipped = null;
                return false;
            }

            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(ResponseBody);
                using var memoryStream = new MemoryStream();
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gzipStream.Write(byteArray, 0, byteArray.Length);
                }

                ResponseBodyZipped = memoryStream.ToArray();
                return true;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("Page SetResponseBodyZipped", exception);
                return false;
            }
        }

        public void SetResponseBodyFromZipped()
        {
            if (ResponseBodyZipped is null)
            {
                return;
            }
            try
            {
                using var memoryStream = new MemoryStream(ResponseBodyZipped);
                using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
                using var streamReader = new StreamReader(gzipStream);
                ResponseBody = streamReader.ReadToEnd();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("Page SetResponseBodyFromZipped", exception);
            }
        }

        public void SetListingStatusPublished()
        {
            ListingStatus = landerist_orels.ES.ListingStatus.published;
        }

        public void SetListingStatusUnpublished()
        {
            ListingStatus = landerist_orels.ES.ListingStatus.unpublished;
        }

        public bool IsListingStatusPublished()
        {
            return ListingStatus == landerist_orels.ES.ListingStatus.published;
        }

        public bool IsListingStatusUnPublished()
        {
            return ListingStatus == landerist_orels.ES.ListingStatus.unpublished;
        }

        public bool ContainsListingStatus()
        {
            return ListingStatus is not null;
        }

        public bool HaveToUnpublishListing()
        {
            return new ListingUnpublishEvaluator(this).ShouldUnpublish();
        }

        public bool IsMayBeListing()
        {
            return PageType == landerist_library.Pages.PageType.MayBeListing;
        }

        public bool IsHttpStatusCodeNotOK()
        {
            return PageType == landerist_library.Pages.PageType.HttpStatusCodeNotOK;
        }

        public bool IsHttpStatusCodeNotFound()
        {
            return HttpStatusCode == 404;
        }

        public bool IsHttpStatusCodeTooManyRequests()
        {
            return HttpStatusCode == 429;
        }

        public bool IsHttpStatusCodeForbidden()
        {
            return HttpStatusCode == 403;
        }

        public bool IsHttpStatusCodeGone()
        {
            return HttpStatusCode == 410;
        }

        public bool IsHttpStatusCodeServerError()
        {
            return HttpStatusCode >= 500 && HttpStatusCode <= 599;
        }

        public bool IsHttpStatusCodeClientError()
        {
            return HttpStatusCode >= 400 && HttpStatusCode <= 499;
        }

        public bool IsResponseBodyNullOrEmpty()
        {
            return PageType == landerist_library.Pages.PageType.ResponseBodyNullOrEmpty;
        }

        public bool IsListing()
        {
            return PageType == landerist_library.Pages.PageType.Listing;
        }

        public bool IsNotListingByParser()
        {
            return PageType == landerist_library.Pages.PageType.NotListingByParser;
        }

        public bool IsNotCanonical()
        {
            return PageType == landerist_library.Pages.PageType.NotCanonical;
        }

        public bool IsRedirectToAnotherUrl()
        {
            return PageType == landerist_library.Pages.PageType.RedirectToAnotherUrl;
        }

        public bool IsDiscardedByListingUrlRegex()
        {
            return PageType == landerist_library.Pages.PageType.DiscardedByListingUrlRegex;
        }

        public bool IsNotCanonicalListing()
        {
            return IsNotCanonical() && ContainsListingStatus();
        }

        public bool IsRedirectToAnotherUrlListing()
        {
            return IsRedirectToAnotherUrl() && ContainsListingStatus();
        }

        public string? GetParseListingUserInput()
        {
            if (!string.IsNullOrEmpty(ParseListingUserInput))
            {
                return ParseListingUserInput;
            }

            ParseListingUserInput = Parse.ListingParser.ParseListingUserInput.GetText(this);
            return ParseListingUserInput;
        }
    }
}
