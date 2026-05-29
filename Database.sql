USE [Landerist]
GO
/****** Object:  Table [dbo].[ADDRESS_CADASTRAL_REFERENCE]    Script Date: 05/05/2026 15:45:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ADDRESS_CADASTRAL_REFERENCE](
	[DateInsert] [datetime] NOT NULL,
	[Address] [nvarchar](200) NOT NULL,
	[CadastralReference] [nvarchar](50) NULL,
 CONSTRAINT [PK_ADDRESS_CADASTRAL_REFERENCE] PRIMARY KEY CLUSTERED 
(
	[Address] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ADDRESS_LAT_LNG]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ADDRESS_LAT_LNG](
	[DateInsert] [datetime] NOT NULL,
	[Address] [nvarchar](100) NOT NULL,
	[Region] [nvarchar](10) NOT NULL,
	[Lat] [float] NOT NULL,
	[Lng] [float] NOT NULL,
	[IsAccurate] [bit] NOT NULL,
 CONSTRAINT [PK_ADDRESS_LAT_LNG] PRIMARY KEY CLUSTERED 
(
	[Address] ASC,
	[Region] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BATCHES]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BATCHES](
	[Created] [datetime] NOT NULL,
	[LLMProvider] [nvarchar](20) NOT NULL,
	[Id] [nvarchar](200) NOT NULL,
	[PagesUriHashes] [nvarchar](max) NOT NULL,
	[Downloaded] [bit] NOT NULL,
 CONSTRAINT [PK_BATCHES] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CNIG]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CNIG](
	[the_geom] [geography] NOT NULL,
	[customid] [int] IDENTITY(10000,1) NOT NULL,
	[inspireid] [nvarchar](50) NOT NULL,
	[natcode] [varchar](50) NOT NULL,
	[nameunit] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_CNIG] PRIMARY KEY CLUSTERED 
(
	[customid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[COUNTRIES]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COUNTRIES](
	[the_geom] [geography] NOT NULL,
	[iso_a3] [nchar](10) NOT NULL,
 CONSTRAINT [PK_COUNTRIES] PRIMARY KEY CLUSTERED 
(
	[iso_a3] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[COUNTRY_SPAIN]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COUNTRY_SPAIN](
	[Id] [nchar](10) NOT NULL,
	[geography] [geography] NOT NULL,
 CONSTRAINT [PK_COUNTRY_SPAIN] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ES_LISTINGS]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ES_LISTINGS](
	[guid] [char](64) NOT NULL,
	[listingStatus] [nvarchar](50) NOT NULL,
	[listingDate] [datetime] NULL,
	[updated] [datetime] NOT NULL,
	[unlistingDate] [datetime] NULL,
	[operation] [nvarchar](50) NOT NULL,
	[propertyType] [nvarchar](50) NOT NULL,
	[propertySubtype] [nvarchar](50) NULL,
	[priceAmount] [float] NULL,
	[priceCurrency] [nvarchar](10) NULL,
	[description] [nvarchar](max) NULL,
	[contactName] [nvarchar](500) NULL,
	[contactPhone] [nvarchar](500) NULL,
	[contactEmail] [nvarchar](500) NULL,
	[contactUrl] [nvarchar](max) NULL,
	[contactOther] [nvarchar](500) NULL,
	[address] [nvarchar](500) NULL,
	[lauId] [varchar](10) NULL,
	[lauName] [nvarchar](200) NULL,
	[latitude] [float] NULL,
	[longitude] [float] NULL,
	[locationIsAccurate] [bit] NULL,
	[cadastralReference] [nvarchar](500) NULL,
	[propertySize] [float] NULL,
	[landSize] [float] NULL,
	[constructionYear] [smallint] NULL,
	[constructionStatus] [nvarchar](50) NULL,
	[floors] [smallint] NULL,
	[floor] [nvarchar](500) NULL,
	[bedrooms] [smallint] NULL,
	[bathrooms] [smallint] NULL,
	[parkings] [smallint] NULL,
	[terrace] [bit] NULL,
	[garden] [bit] NULL,
	[garage] [bit] NULL,
	[motorbikeGarage] [bit] NULL,
	[pool] [bit] NULL,
	[lift] [bit] NULL,
	[disabledAccess] [bit] NULL,
	[storageRoom] [bit] NULL,
	[furnished] [bit] NULL,
	[nonFurnished] [bit] NULL,
	[heating] [bit] NULL,
	[airConditioning] [bit] NULL,
	[petsAllowed] [bit] NULL,
	[securitySystems] [bit] NULL,
	[host] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_ES_LISTINGS] PRIMARY KEY CLUSTERED 
(
	[guid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ES_MEDIA]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ES_MEDIA](
	[listingGuid] [char](64) NOT NULL,
	[mediaType] [nvarchar](50) NULL,
	[title] [nvarchar](500) NULL,
	[url] [nvarchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ES_SOURCES]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ES_SOURCES](
	[listingGuid] [char](64) NOT NULL,
	[sourceName] [nvarchar](500) NULL,
	[sourceUrl] [nvarchar](max) NOT NULL,
	[sourceGuid] [nvarchar](500) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FT_AGENCIES_URLS]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FT_AGENCIES_URLS](
	[Url] [nvarchar](400) NOT NULL,
	[AgencyUrl] [nvarchar](500) NULL,
 CONSTRAINT [PK_FT_AGENCIES_URLS] PRIMARY KEY CLUSTERED 
(
	[Url] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GLOBAL_STATISTICS]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GLOBAL_STATISTICS](
	[Date] [datetime] NOT NULL,
	[Key] [nvarchar](50) NOT NULL,
	[Counter] [bigint] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HOST_STATISTICS]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HOST_STATISTICS](
	[Date] [datetime] NOT NULL,
	[Host] [nvarchar](200) NOT NULL,
	[Key] [nvarchar](200) NOT NULL,
	[Counter] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ID_AGENCIES_URLS]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ID_AGENCIES_URLS](
	[Url] [nvarchar](400) NOT NULL,
	[Province] [smallint] NOT NULL,
	[AgencyUrl] [nvarchar](500) NULL,
 CONSTRAINT [PK_AGENCIES_URLS] PRIMARY KEY CLUSTERED 
(
	[Url] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[INVALID_IMAGES]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[INVALID_IMAGES](
	[DateInsert] [datetime] NOT NULL,
	[UriHash] [char](64) NOT NULL,
 CONSTRAINT [PK_DISCARDED_IMAGES] PRIMARY KEY CLUSTERED 
(
	[UriHash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LAU]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LAU](
	[the_geom] [geography] NOT NULL,
	[gisco_id] [varchar](50) NOT NULL,
	[lau_id] [varchar](50) NOT NULL,
	[lau_name] [nvarchar](150) NOT NULL,
 CONSTRAINT [PK_LAU] PRIMARY KEY CLUSTERED 
(
	[gisco_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LOGS]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LOGS](
	[IdLog] [bigint] IDENTITY(1000000,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[MachineName] [varchar](50) NOT NULL,
	[LogKey] [varchar](50) NOT NULL,
	[Source] [nvarchar](max) NOT NULL,
	[Text] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_TABLAGOOLZOOMLOG] PRIMARY KEY CLUSTERED 
(
	[IdLog] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NOT_LISTINGS_CACHE]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NOT_LISTINGS_CACHE](
	[Inserted] [datetime] NOT NULL,
	[Host] [nvarchar](200) NOT NULL,
	[ListingParserInputHash] [char](64) NOT NULL,
 CONSTRAINT [PK_NOT_LISTINGS_ALREADY_PARSED] PRIMARY KEY CLUSTERED 
(
	[Host] ASC,
	[ListingParserInputHash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PAGES]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PAGES](
	[Host] [nvarchar](200) NOT NULL,
	[Uri] [nvarchar](max) NOT NULL,
	[UriHash] [char](64) NOT NULL,
	[Inserted] [datetime] NOT NULL,
	[LastScrape] [datetime] NULL,
	[NextScrape] [datetime] NULL,
	[HttpStatusCode] [smallint] NULL,
	[PageType] [varchar](50) NULL,
	[PageTypeCounter] [smallint] NULL,
	[ListingStatus] [varchar](50) NULL,
	[LockedBy] [varchar](50) NULL,
	[WaitingStatus] [varchar](50) NULL,
	[ListingParserInputHash] [char](64) NULL,
	[ResponseBodyZipped] [varbinary](max) NULL,
	[TokenCount] [int] NULL,
	[ListingParserInputNotChangedCounter] [smallint] NULL,
	[TransientErrorCounter] [smallint] NULL,
	[Etag] [varchar](512) NULL,
	[LastModified] [varchar](512) NULL,
 CONSTRAINT [PK_PAGES] PRIMARY KEY CLUSTERED 
(
	[UriHash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[REDIRECT_URL]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[REDIRECT_URL](
	[DateInsert] [datetime] NOT NULL,
	[OriginalUrl] [nvarchar](max) NOT NULL,
	[RedirectUrl] [nvarchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TIMERS]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TIMERS](
	[IdTimer] [bigint] IDENTITY(1000000,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[TimerKey] [varchar](50) NOT NULL,
	[Source] [nvarchar](max) NOT NULL,
	[Milliseconds] [float] NOT NULL,
 CONSTRAINT [PK_TIMERS] PRIMARY KEY CLUSTERED 
(
	[IdTimer] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TRAINING_DATA]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TRAINING_DATA](
	[UriHash] [char](64) NOT NULL,
	[ResponseBodyTextHash] [char](64) NOT NULL,
	[ResponseBodyZipped] [varbinary](max) NOT NULL,
	[IsListing] [bit] NOT NULL,
 CONSTRAINT [PK_TRAINING_DATA] PRIMARY KEY CLUSTERED 
(
	[UriHash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [IX_TRAINING_DATA] UNIQUE NONCLUSTERED 
(
	[ResponseBodyTextHash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VALID_IMAGES]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VALID_IMAGES](
	[DateInsert] [datetime] NOT NULL,
	[UriHash] [char](64) NOT NULL,
 CONSTRAINT [PK_VALID_IMAGES] PRIMARY KEY CLUSTERED 
(
	[UriHash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WEBSITES]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WEBSITES](
	[MainUri] [nvarchar](max) NOT NULL,
	[Host] [nvarchar](200) NOT NULL,
	[LanguageCode] [nvarchar](10) NOT NULL,
	[CountryCode] [nvarchar](10) NOT NULL,
	[RobotsTxt] [text] NULL,
	[RobotsTxtUpdated] [datetime] NULL,
	[SitemapUpdated] [datetime] NULL,
	[IpAddress] [nvarchar](50) NULL,
	[IpAddressUpdated] [datetime] NULL,
	[ListingUrlRegex] [nvarchar](500) NULL,
	[ApplySpecialRules] [bit] NULL,
	[IndexUrlRegex] [nvarchar](500) NULL,
	[SitemapUrlRegex] [nvarchar](500) NULL,
	[HtmlIndexingEnabled] [bit] NOT NULL,
	[ListingHtmlRemoveXPath] [nvarchar](max) NULL,
	[AllowedResourceTypes] [nvarchar](500) NULL,
	[UserAgent] [nvarchar](500) NULL,
	[HttpRequestHeaders] [nvarchar](max) NULL,
	[UseProxy] [bit] NOT NULL,
 CONSTRAINT [PK_WEBSITES] PRIMARY KEY CLUSTERED 
(
	[Host] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WEBSITES_THROTTLE]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WEBSITES_THROTTLE](
	[BlockUntil] [datetime] NOT NULL,
	[IpOrHost] [nvarchar](100) NOT NULL,
	[ForbiddenBackoffLevel] [smallint] NOT NULL,
	[ForbiddenRetryDelaySeconds] [int] NOT NULL,
	[ForbiddenCounter] [int] NOT NULL,
	[SuccessCounterAfterForbidden] [int] NOT NULL,
	[LastForbiddenAt] [datetime] NULL,
	[LastSuccessAt] [datetime] NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_IPHOST_BLOCKER] PRIMARY KEY CLUSTERED 
(
	[IpOrHost] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WORDS]    Script Date: 05/05/2026 15:45:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WORDS](
	[Word] [nvarchar](200) NOT NULL,
	[Counter] [bigint] NOT NULL,
 CONSTRAINT [PK_WORDS] PRIMARY KEY CLUSTERED 
(
	[Word] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[WEBSITES] ADD  CONSTRAINT [DF_WEBSITES_HtmlIndexingEnabled]  DEFAULT ((1)) FOR [HtmlIndexingEnabled]
GO
ALTER TABLE [dbo].[WEBSITES] ADD  CONSTRAINT [DF_WEBSITES_UseProxy]  DEFAULT ((0)) FOR [UseProxy]
GO
ALTER TABLE [dbo].[WEBSITES_THROTTLE] ADD  CONSTRAINT [DF_WEBSITES_BLOCKER_ForbiddenBackoffLevel]  DEFAULT ((0)) FOR [ForbiddenBackoffLevel]
GO
ALTER TABLE [dbo].[WEBSITES_THROTTLE] ADD  CONSTRAINT [DF_WEBSITES_BLOCKER_ForbiddenRetryDelaySeconds]  DEFAULT ((0)) FOR [ForbiddenRetryDelaySeconds]
GO
ALTER TABLE [dbo].[WEBSITES_THROTTLE] ADD  CONSTRAINT [DF_WEBSITES_BLOCKER_ForbiddenCounter]  DEFAULT ((0)) FOR [ForbiddenCounter]
GO
ALTER TABLE [dbo].[WEBSITES_THROTTLE] ADD  CONSTRAINT [DF_WEBSITES_BLOCKER_SuccessCounterAfterForbidden]  DEFAULT ((0)) FOR [SuccessCounterAfterForbidden]
GO
ALTER TABLE [dbo].[WEBSITES_THROTTLE] ADD  CONSTRAINT [DF_WEBSITES_BLOCKER_Updated]  DEFAULT (getdate()) FOR [Updated]
GO
