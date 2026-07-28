using landerist_orels;
using System.Data;

namespace landerist_library.Infrastructure.Sql.Mapping;

public static class MediaDataMapper
{
    public static Media? Map(DataRow row)
    {
        if (!Uri.TryCreate((string)row["url"], UriKind.Absolute, out Uri? uri))
        {
            return null;
        }

        return new Media
        {
            mediaType = row["mediaType"] is DBNull
                ? null
                : Enum.Parse<MediaType>(row["mediaType"].ToString()!),
            title = row["title"] is DBNull ? null : (string)row["title"],
            url = uri
        };
    }
}
