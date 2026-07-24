using landerist_orels;
using System.Data;

namespace landerist_library.Infrastructure.Sql.Mapping;

public static class SourceDataMapper
{
    public static Source? Map(DataRow row)
    {
        if (!Uri.TryCreate((string)row["sourceUrl"], UriKind.Absolute, out Uri? uri))
        {
            return null;
        }

        return new Source
        {
            sourceName = row["sourceName"] is DBNull ? null : (string)row["sourceName"],
            sourceUrl = uri,
            sourceGuid = row["sourceGuid"] is DBNull ? null : (string)row["sourceGuid"]
        };
    }
}
