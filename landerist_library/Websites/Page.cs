using System.Data;

namespace landerist_library.Websites
{
    internal class Page : WebBase
    {
        public Guid PageGuid { get; set; }        

        public string Domain { get; set; }

        public Uri Uri { get; set; }

        public DateTime Inserted { get; set; }

        public DateTime Updated { get; set; }

        public bool? IsAdvertisement { get; set; } = null;

        public Page(DataRow dataRow)
        {
            PageGuid = (Guid)dataRow["PageGuid"];
            Domain = dataRow["Domain"].ToString()!;
            string uriString = dataRow["Uri"].ToString()!;
            Uri = new Uri(uriString);
            IsAdvertisement = dataRow["IsAdvertisement"] is DBNull ? null : (bool)dataRow["IsAdvertisement"];
            Inserted = (DateTime)dataRow["Inserted"];
            Updated = (DateTime)dataRow["Updated"];
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + PAGES + " " +
                "VALUES(@Domain, @Uri, @Inserted, @Updated, @IsAdvertisement)";

            return new Database().Query(query, new Dictionary<string, object> {
                {"PageGuid", PageGuid },
                {"Domain", Domain },
                {"Uri", Uri },
                {"Inserted", Inserted },
                {"Updated", Updated },
                {"IsAdvertisement", IsAdvertisement },
            });
        }
    }
}
