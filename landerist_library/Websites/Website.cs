using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace landerist_library.Websites
{
    public class Website
    {
        private readonly string TABLE_WEBSITES = "[WEBSITES]";
        public Uri Uri { get; set; }

        public string Host { get; set; } = string.Empty;

        public string? Robotstxt { get; set; }

        public string? IpAddress { get; set; }

        public Website(Uri uri)
        {
            Uri = uri;
            Host = uri.Host;
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + TABLE_WEBSITES + " " +
                "VALUES (@Uri, @Host, NULL, NULL)";

            return new Database().Query(query, new Dictionary<string, object> {
                {"Uri", Uri.ToString() },
                {"Host", Host }
            });
        }
    }
}
