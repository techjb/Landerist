using Microsoft.Data.SqlClient;
using System.Data;

namespace landerist_library.Database;

internal static class SqlDataReaderMapper
{
    public static List<T> ReadList<T>(SqlCommand command, Func<SqlDataReader, T> map)
    {
        List<T> values = [];
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            values.Add(map(reader));
        }

        return values;
    }

    public static async Task<DataTable> ReadTableAsync(
        SqlCommand command,
        CancellationToken cancellationToken)
    {
        await using SqlDataReader reader = await command
            .ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        DataTable table = new();
        for (int index = 0; index < reader.FieldCount; index++)
        {
            table.Columns.Add(reader.GetName(index), reader.GetFieldType(index));
        }

        object[] values = new object[reader.FieldCount];
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            reader.GetValues(values);
            table.Rows.Add((object[])values.Clone());
        }

        return table;
    }
}
