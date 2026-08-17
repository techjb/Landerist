using Microsoft.Data.SqlClient;

namespace landerist_library.Database;

internal static class SqlParameterBinder
{
    public static SqlParameter Create(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        string parameterName = name.StartsWith('@') ? name : "@" + name;
        return new SqlParameter(parameterName, value ?? DBNull.Value);
    }

    public static void AddTo(
        SqlCommand command,
        IDictionary<string, object?>? parameters,
        SqlParameter[]? sqlParameters)
    {
        if (parameters is not null)
        {
            foreach (var item in parameters)
            {
                command.Parameters.Add(Create(item.Key, item.Value));
            }
        }

        if (sqlParameters is null)
        {
            return;
        }

        foreach (SqlParameter parameter in sqlParameters)
        {
            command.Parameters.Add((SqlParameter)((ICloneable)parameter).Clone());
        }
    }
}
