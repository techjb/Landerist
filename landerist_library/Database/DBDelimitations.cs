using System.Data;
using System.Globalization;

namespace landerist_library.Database
{
    public class DBDelimitations
    {
        protected static bool DeleteAll(string tableName)
        {
            string query = "DELETE FROM " + tableName;
            return LegacyDatabase.Create().Query(query);
        }

        protected static bool MakeValidTheGeom(string tableName)
        {
            string query =
                "UPDATE " + tableName + " " +
                "SET [the_geom] = [the_geom].MakeValid()";

            return LegacyDatabase.Create().Query(query);
        }

        protected static bool ReorientTheGeomIfNeccesary(string tableName)
        {
            string query =
                "UPDATE " + tableName + " " +
                "SET [the_geom] = [the_geom].ReorientObject().MakeValid() " +
                "WHERE [the_geom].EnvelopeAngle() > 90";

            return LegacyDatabase.Create().Query(query);
        }

        protected static string? GetString(string tableName, string columnName, double latitude, double longitude)
        {
            string point =
                "POINT(" + longitude.ToString(CultureInfo.InvariantCulture) + " " +
                latitude.ToString(CultureInfo.InvariantCulture) + ")";

            string query =
                "SELECT TOP 1 " + columnName + " " +
                "FROM " + tableName + " " +
                "WITH(INDEX([SpatialIndex-the_geom])) " +
                "WHERE [the_geom].STIntersects(geography::STGeomFromText('" + point + "', 4326)) = 1";

            return LegacyDatabase.Create().QueryString(query);
        }

        protected static DataRow? GetdDataRow(string tableName, string columns, double latitude, double longitude)
        {
            string point =
                "POINT(" + longitude.ToString(CultureInfo.InvariantCulture) + " " +
                latitude.ToString(CultureInfo.InvariantCulture) + ")";

            string query =
                "SELECT TOP 1  " + columns + " " +
                "FROM " + tableName + " " +
                "WITH(INDEX([SpatialIndex-the_geom])) " +
                "WHERE [the_geom].STIntersects(geography::STGeomFromText('" + point + "', 4326)) = 1";

            DataTable dataTable = LegacyDatabase.Create().QueryTable(query);
            if (dataTable.Rows.Count > 0)
            {
                return dataTable.Rows[0];
            }
            return null;
        }
    }
}
