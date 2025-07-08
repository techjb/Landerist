using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Downloaders;
using landerist_library.Index;
using landerist_library.Tools;
using landerist_orels.ES;
using System.Data;
using System.IO.Compression;
using System.Text;

namespace landerist_library.Websites
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

        public PageType? PageType { get; private set; }

        public short? PageTypeCounter { get; private set; }

        private ListingStatus? ListingStatus { get; set; }

        public string? LockedBy { get; set; }

        public WaitingStatus? WaitingStatus { get; private set; }

        private string? ResponseBody { get; set; }

        public string? ResponseBodyText { get; set; }

        public string? ResponseBodyTextHash { get; set; }

        public byte[]? ResponseBodyZipped { get; private set; }

        public bool ResponseBodyTextNotChanged { get; set; } = false;

        public byte[]? Screenshot { get; set; }


        private HtmlDocument? HtmlDocument = null;


        private string? OriginalOuterHtml = null;


        public Website Website = new();


        private bool Disposed;

        public Page(string url) : this(new Uri(url))
        {

        }

        public Page(Uri uri) : this(Websites.GetWebsite(uri.Host), uri)
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
            UriHash = Strings.GetHash(uri.ToString());
            Inserted = DateTime.Now;
            Updated = DateTime.Now;

            var dataRow = GetDataRow();
            if (dataRow != null)
            {
                Load(dataRow);
            }
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
            PageType = dataRow["PageType"] is DBNull ? null : (PageType)Enum.Parse(typeof(PageType), dataRow["PageType"].ToString()!);
            PageTypeCounter = dataRow["PageTypeCounter"] is DBNull ? null : (short)dataRow["PageTypeCounter"];
            ListingStatus = dataRow["ListingStatus"] is DBNull ? null : (ListingStatus)Enum.Parse(typeof(ListingStatus), dataRow["ListingStatus"].ToString()!);
            LockedBy = dataRow["LockedBy"] is DBNull ? null : dataRow["LockedBy"].ToString();
            WaitingStatus = dataRow["WaitingStatus"] is DBNull ? null : (WaitingStatus)Enum.Parse(typeof(WaitingStatus), dataRow["WaitingStatus"].ToString()!);
            ResponseBodyTextHash = dataRow["ResponseBodyTextHash"] is DBNull ? null : dataRow["ResponseBodyTextHash"].ToString();
            ResponseBodyZipped = dataRow["ResponseBodyZipped"] is DBNull ? null : (byte[])dataRow["ResponseBodyZipped"];
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
                "INSERT INTO " + Pages.PAGES + " " +
                "VALUES(@Host, @Uri, @UriHash, @Inserted, @Updated, @NextUpdate, @HttpStatusCode, @PageType, " +
                "@PageTypeCounter, @ListingStatus, @LockedBy, @WaitingStatus, @ResponseBodyTextHash, " +
                "CONVERT(varbinary(max), @ResponseBodyZipped))";

            bool sucess = new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", Host },
                {"Uri", Uri.ToString() },
                {"UriHash", UriHash },
                {"Inserted", DateTime.Now },
                {"Updated", null },
                {"NextUpdate", null },
                {"HttpStatusCode", null },
                {"PageType", null },
                {"PageTypeCounter", null },
                {"ListingStatus", null },
                {"LockedBy", null },
                {"WaitingStatus", null },
                {"ResponseBodyTextHash", null },
                {"ResponseBodyZipped", null  },
            });
            if (sucess)
            {
                Website.IncreaseNumPages();
            }
            //else
            //{
            //    Logs.Log.WriteError("Page Insert", "Failed to insert page: " + Uri);
            //}
            return sucess;
        }


        public bool Update(PageType? pageType, bool setNextUpdate)
        {
            SetPageType(pageType);
            return Update(setNextUpdate);
        }

        public bool Update(bool setNextUpdate)
        {
            //if (Config.IsConfigurationLocal())
            //{
            //    return true;
            //}

            Updated = DateTime.Now;
            if (setNextUpdate)
            {
                SetNextUpdate();
            }

            string query =
                "UPDATE " + Pages.PAGES + " SET " +
                "[Updated] = @Updated, " +
                "[NextUpdate] = @NextUpdate, " +
                "[HttpStatusCode] = @HttpStatusCode, " +
                "[PageType] = @PageType, " +
                "[PageTypeCounter] = @PageTypeCounter, " +
                "[ListingStatus] = @ListingStatus, " +
                "[LockedBy] = @LockedBy, " +
                "[WaitingStatus] = @WaitingStatus, " +
                "[ResponseBodyTextHash] = @ResponseBodyTextHash, " +
                "[ResponseBodyZipped] = CASE WHEN @ResponseBodyZipped IS NULL THEN NULL ELSE CONVERT(varbinary(max), @ResponseBodyZipped) END " +
                "WHERE [UriHash] = @UriHash";

            var sucess = new DataBase().Query(query, new Dictionary<string, object?> {
                {"Updated", Updated },
                {"NextUpdate", NextUpdate },
                {"HttpStatusCode", HttpStatusCode},
                {"PageType", PageType?.ToString()},
                {"PageTypeCounter", PageTypeCounter},
                {"ListingStatus", ListingStatus?.ToString()},
                {"LockedBy", LockedBy?.ToString()},
                {"WaitingStatus", WaitingStatus?.ToString()},
                {"ResponseBodyTextHash", ResponseBodyTextHash},
                {"ResponseBodyZipped", ResponseBodyZipped},
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
            if (PageType == null || PageTypeCounter == null)
            {
                NextUpdate = null;
            }

            var addDays = PageType switch
            {
                landerist_library.Websites.PageType.MainPage => Config.DEFAULT_DAYS_NEXT_UPDATE,
                landerist_library.Websites.PageType.MayBeListing => Config.DEFAULT_DAYS_NEXT_UPDATE,
                landerist_library.Websites.PageType.Listing => Config.DEFAULT_DAYS_NEXT_UPDATE_LISTING,
                _ => (short)PageTypeCounter! * Config.DEFAULT_DAYS_NEXT_UPDATE,
            };

            if (IsListingStatusPublished())
            {
                addDays = Config.DEFAULT_DAYS_NEXT_UPDATE_LISTING;
            }

            addDays = Math.Clamp(addDays, Config.MIN_DAYS_NEXT_UPDATE, Config.MAX_DAYS_NEXT_UPDATE);
            NextUpdate = DateTime.Now!.AddDays(addDays);
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
                Website.DecreaseNumPages() &&
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
                    Website.DecreaseNumListings();
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

        public bool MainPage()
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
            ResponseBody = downloader.Content;
            ResponseBodyText = null;
            Screenshot = downloader.Screenshot;
            HttpStatusCode = downloader.HttpStatusCode;
        }

        public void SetResponseBodyText()
        {
            var htmlDocument = GetHtmlDocument();
            if (htmlDocument == null)
            {
                return;
            }
            ResponseBodyText = HtmlToText.GetText(htmlDocument);
            if (string.IsNullOrEmpty(ResponseBodyText))
            {
                return;
            }
            string hash = Strings.GetHash(ResponseBodyText);
            ResponseBodyTextNotChanged = hash == ResponseBodyTextHash;
            ResponseBodyTextHash = hash;
        }


        public bool ResponseBodyTextAlreadyParsed()
        {
            return ResponseBodyTextNotChanged && (IsListing() || IsNotListingByParser());
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
                ResponseBodyText.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) ||
                ResponseBodyText.Contains("no existe", StringComparison.OrdinalIgnoreCase) ||
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

        public bool ResponseBodyTextAlreadyParsedToNotListing()
        {
            if (string.IsNullOrEmpty(ResponseBodyTextHash))
            {
                return false;
            }
            return NotListings.IsNotListing(ResponseBodyTextHash);
        }

        public bool InsertNotListingResponseBodyText()
        {
            return (ResponseBodyTextHash != null) && NotListings.Insert(ResponseBodyTextHash);
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
            if (canonicalUri != null)
            {
                return !UrisAreEquals(Uri, canonicalUri);
            }
            return false;
        }

        public static bool UrisAreEquals(Uri uri1, Uri uri2)
        {
            string normalizedUri1 = uri1.ToString().TrimEnd('/');
            string normalizedUri2 = uri2.ToString().TrimEnd('/');

            return normalizedUri1.Equals(normalizedUri2, StringComparison.OrdinalIgnoreCase)
                && uri1.Host.Equals(uri2.Host, StringComparison.OrdinalIgnoreCase)
                && uri1.Scheme.Equals(uri2.Scheme, StringComparison.OrdinalIgnoreCase)
                && uri1.Port == uri2.Port;
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
                ResponseBodyZipped = null;
                Screenshot = null;

                Website.Dispose();
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
            SetWaitingStatus(landerist_library.Websites.WaitingStatus.waiting_ai_request);
        }

        public void SetWaitingStatusAIResponse()
        {
            SetWaitingStatus(landerist_library.Websites.WaitingStatus.waiting_ai_response);
        }

        public bool IsWaitingForAIResponse()
        {
            return WaitingStatus is not null && WaitingStatus == landerist_library.Websites.WaitingStatus.waiting_ai_response;
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
            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(ResponseBody!);
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

        public async void SetResponseBodyFromZipped()
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
                ResponseBody = await streamReader.ReadToEndAsync();
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
            return IsListingStatusPublished() &&
                !IsMayBeListing() &&
                !IsListing() &&
                PageTypeCounter >= Config.MINIMUM_PAGE_TYPE_COUNTER_TO_UNPUBLISH_LISTING;
        }

        public bool IsMayBeListing()
        {
            return PageType == landerist_library.Websites.PageType.MayBeListing;
        }

        public bool IsHttpStatusCodeNotOK()
        {
            return PageType == landerist_library.Websites.PageType.HttpStatusCodeNotOK;
        }

        public bool IsResponseBodyNullOrEmpty()
        {
            return PageType == landerist_library.Websites.PageType.ResponseBodyNullOrEmpty;
        }

        public bool IsListing()
        {
            return PageType == landerist_library.Websites.PageType.Listing;
        }

        public bool IsNotListingByParser()
        {
            return PageType == landerist_library.Websites.PageType.NotListingByParser;
        }

        public bool IsNotCanonical()
        {
            return PageType == landerist_library.Websites.PageType.NotCanonical;
        }

        public bool IsNotCanonicalListing()
        {
            return IsNotCanonical() && ContainsListingStatus();
        }
    }
}
