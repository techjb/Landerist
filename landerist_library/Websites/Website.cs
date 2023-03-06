using System.Data;
using System.Net;

namespace landerist_library.Websites
{
    public class Website : Websites
    {
        public Uri Uri { get; set; }

        public string Domain { get; set; } = string.Empty;

        public string? RobotsTxt { get; set; }

        public string? IpAddress { get; set; }

        public short? StatusCode { get; set; }

        public Website(Uri uri)
        {
            Uri = uri;
            Domain = uri.Host;
        }

        public Website(DataRow dataRow)
        {
            string uriString = dataRow["Uri"].ToString()!;
            Uri = new(uriString);
            Domain = dataRow["Domain"].ToString()!;
            RobotsTxt = dataRow["RobotsTxt"].ToString();
            IpAddress = dataRow["IpAddress"].ToString();
            StatusCode = dataRow["StatusCode"] is DBNull ? null : (short)dataRow["StatusCode"];
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + TABLE_WEBSITES + " " +
                "VALUES (@Uri, @Domain, NULL, NULL, NULL)";

            return new Database().Query(query, new Dictionary<string, object> {
                {"Uri", Uri.ToString() },
                {"Domain", Domain }
            });
        }

        public bool UpdateRobotsTxt(string robotsTxt)
        {
            RobotsTxt = robotsTxt;
            if (RobotsTxt == null)
            {
                return true;
            }

            string query =
                "UPDATE " + TABLE_WEBSITES + " " +
                "SET RobotsTxt =  @RobotsTxt " +
                "WHERE Domain = @Domain";

            return new Database().Query(query, new Dictionary<string, object> {
                {"RobotsTxt", RobotsTxt },
                {"Domain", Domain }
            });
        }

        public bool UpdateIpAddress(string ipAddress)
        {
            IpAddress = ipAddress;
            if (IpAddress == null)
            {
                return true;
            }
            string query =
                "UPDATE " + TABLE_WEBSITES + " " +
                "SET IpAddress =  @IpAddress " +
                "WHERE Domain = @Domain";

            return new Database().Query(query, new Dictionary<string, object> {
                {"IpAddress", IpAddress },
                {"Domain", Domain }
            });
        }

        public bool UpdateUri(Uri uri)
        {
            Uri = uri;
            var newDomain = uri.Host;
            string query =
                "UPDATE " + TABLE_WEBSITES + " " +
                "SET Uri =  @Uri, Domain  = @NewDomain " +
                "WHERE Domain = @Domain";

            var dataBase = new Database();

            bool success = dataBase.Query(query, new Dictionary<string, object> {
                {"Uri", Uri.ToString() },
                {"Domain", Domain },
                {"NewDomain", newDomain },
            });

            if (!success) // newDomain is already in database
            {
                success = dataBase.Query(query, new Dictionary<string, object> {
                    {"Uri", Uri.ToString() },
                    {"Domain", newDomain },
                    {"NewDomain", newDomain },
            });
            }
            if (success)
            {
                Domain = newDomain;
            }
            return success;
        }

        public bool UpdateUriStatusCode(short statusCode)
        {
            StatusCode = statusCode;
            if (StatusCode == null)
            {
                return true;
            }
            string query =
                "UPDATE " + TABLE_WEBSITES + " " +
                "SET StatusCode =  @StatusCode " +
                "WHERE Domain = @Domain";

            return new Database().Query(query, new Dictionary<string, object> {
                {"StatusCode", StatusCode },
                {"Domain", Domain }
            });
        }
    }
}
