using landerist_library.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;

namespace landerist_library.Database
{
    public class DataBase(string connectionString)
    {
        #region Private Variables

        private SqlConnection SqlConnection = new();
        private SqlCommand SqlCommand = new();
        private readonly SqlDataAdapter SqlDataAdapter = new();
        private readonly StringBuilder StringBuilder = new();
        private int StackCounter = 0;
        private int TimeOut = 120;
        public string ConnectionString = connectionString;

        #endregion Private Variables

        #region Constructors

        public DataBase() : this(Config.DATABASE_USER, Config.DATABASE_PW, Config.DATABASE_NAME)
        {

        }

        public DataBase(string userId, string pw, string databaseName) :
            this("User ID=" + userId + ";" +
                "Password=" + pw + ";" +
                "Initial Catalog=" + databaseName + ";" +
                "Connect Timeout=100000;" +
                "Data Source=" + Config.DATASOURCE + ";" +
                "TrustServerCertificate=True;")
        {

        }

        #endregion Constructors

        #region Private Methods

        private void InitialiceConnection()
        {
            SqlConnection = new SqlConnection(ConnectionString);
            SqlCommand = new SqlCommand
            {
                Connection = SqlConnection,
                CommandTimeout = TimeOut
            };
        }

        private void CloseConnection()
        {
            if (SqlConnection.State == ConnectionState.Open)
            {
                SqlConnection.Close();
            }

            SqlConnection.Dispose();
            SqlCommand.Dispose();
        }

        private void AddParameters(IDictionary<string, object?>? parameters = null)
        {
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object?> item in parameters)
                {
                    string key = item.Key;
                    SqlCommand.Parameters.AddWithValue("@" + key, item.Value ?? DBNull.Value);
                }
            }
        }

        private void AddParameters(SqlParameter[]? sqlParameters = null)
        {
            if (sqlParameters != null)
            {
                SqlCommand.Parameters.AddRange(sqlParameters);
            }
        }

        private void InitConnectionAndComand(string query)
        {
            InitialiceConnection();
            SqlCommand.CommandText = query;
        }

        private void Init(string query)
        {
            InitConnectionAndComand(query);
            SqlConnection.Open();
        }

        private void Init(string query, IDictionary<string, object?>? parameters = null)
        {
            InitConnectionAndComand(query);
            AddParameters(parameters);
            SqlConnection.Open();
        }

        private void Init(string query, IDictionary<string, object?>? parameters = null,
            SqlParameter[]? sqlParameters = null)
        {
            InitConnectionAndComand(query);
            AddParameters(parameters);
            AddParameters(sqlParameters);
            SqlConnection.Open();
        }

        private void Init(string query, SqlParameter[]? sqlParameters = null)
        {
            InitConnectionAndComand(query);
            AddParameters(sqlParameters);
            SqlConnection.Open();
        }

        #endregion Private Methods

        #region Modify connection

        /// <summary>
        /// Sets the database timeout in secconds.
        /// </summary>
        /// <param name="timeOut">Timeout in seconds</param>
        public void SetTimeout(int timeOut)
        {
            TimeOut = timeOut;
        }

        #endregion

        #region Queries

        public static List<SqlParameter> ParseParameters(Dictionary<string, object?> parameters)
        {
            List<SqlParameter> list = [];
            foreach (KeyValuePair<string, object?> item in parameters)
            {
                var parameter = new SqlParameter("@" + item.Key, item.Value ?? DBNull.Value);
                list.Add(parameter);
            }
            return list;
        }
        public bool Query(string query)
        {
            bool result = false;
            try
            {
                Init(query);
                SqlCommand.ExecuteNonQuery();
                result = true;
            }
            catch
            {

            }
            CloseConnection();
            return result;
        }

        public bool Query(string query, string parameterName, object parameterValue)
        {
            var dictionary = new Dictionary<string, object?> {
                {
                    parameterName, parameterValue
                }
            };
            return Query(query, dictionary);
        }

        public bool Query(string query, IDictionary<string, object?>? parameters = null)
        {
            bool result = false;
            try
            {
                Init(query, parameters);
                SqlCommand.ExecuteNonQuery();
                result = true;
            }
            catch (Exception exception)
            {
                exception.ToString();
            }
            CloseConnection();
            return result;
        }

        public bool Query(string query, List<SqlParameter> sqlParameters)
        {
            return Query(query, sqlParameters.ToArray());
        }


        public bool Query(string query, SqlParameter[] sqlParameters)
        {
            bool result = false;
            try
            {
                Init(query, sqlParameters);
                SqlCommand.ExecuteNonQuery();
                result = true;
            }
            catch (Exception exception)
            {
                exception.ToString();
            }
            CloseConnection();
            return result;
        }

        public bool QueryBool(string query, IDictionary<string, object?>? parameters = null)
        {
            bool result = false;
            try
            {
                Init(query, parameters);
                var value = SqlCommand.ExecuteScalar();
                if (value != null)
                {
                    _ = bool.TryParse(value.ToString(), out result);
                }
            }
            catch (Exception exception)
            {
                exception.ToString();
            }
            CloseConnection();
            return result;
        }

        public bool QueryExists(string querySelect1, IDictionary<string, object?>? parameters = null)
        {
            string query =
                "SELECT CASE " +
                $"WHEN EXISTS ({querySelect1}) THEN CAST(1 AS BIT) " +
                "ELSE CAST(0 AS BIT) " +
                "END";

            return QueryBool(query, parameters);
        }

        public int QueryInt(string query, IDictionary<string, object?>? parameters = null)
        {
            int result = 0;
            try
            {
                Init(query, parameters);
                var value = SqlCommand.ExecuteScalar();
                if (value != null)
                {
                    _ = int.TryParse(value.ToString(), out result);
                }
            }
            catch (Exception exception)
            {
                exception.ToString();
            }
            CloseConnection();
            return result;
        }

        public long QueryLong(string query, IDictionary<string, object?>? parameters = null)
        {
            long result = 0;
            try
            {
                Init(query, parameters);
                var value = SqlCommand.ExecuteScalar();
                if (value != null)
                {
                    _ = long.TryParse(value.ToString(), out result);
                }
            }
            catch (Exception exception)
            {
                exception.ToString();
            }
            CloseConnection();
            return result;
        }

        public string? QueryString(string query, IDictionary<string, object?>? parameters = null)
        {
            string? result = null;
            try
            {
                Init(query, parameters);
                var value = SqlCommand.ExecuteScalar();
                if (value != null)
                {
                    result = (string)value;
                }
            }
            catch (Exception exception)
            {
                exception.ToString();
            }
            CloseConnection();
            return result;
        }

        public Guid QueryGuid(string query, IDictionary<string, object?>? parameters = null)
        {
            Guid result = Guid.Empty;
            try
            {
                Init(query, parameters);
                var value = SqlCommand.ExecuteScalar();
                if (value != null)
                {
                    _ = Guid.TryParse(value.ToString(), out result);
                }
            }
            catch (Exception exception)
            {
                exception.ToString();
            }
            CloseConnection();
            return result;
        }

        public DataTable QueryTable(string query, string parameterName, object? parameterValue)
        {
            return QueryTable(query, new Dictionary<string, object?> {
                { parameterName, parameterValue } }
            );
        }

        public DataTable QueryTable(string query,
            IDictionary<string, object?>? parameters = null, SqlParameter[]? sqlParameters = null)
        {
            DataTable dataTable = new();
            try
            {
                Init(query, parameters, sqlParameters);
                SqlDataAdapter.SelectCommand = SqlCommand;
                SqlDataAdapter.Fill(dataTable);
            }
            catch
            (Exception exception)
            {
                exception.ToString();
            }
            CloseConnection();
            return dataTable;
        }

        public DataSet QueryDataSet(string query,
            IDictionary<string, object?>? parameters = null, SqlParameter[]? sqlParameters = null)
        {
            DataSet dataSet = new();
            try
            {
                Init(query, parameters, sqlParameters);
                SqlDataAdapter.SelectCommand = SqlCommand;
                SqlDataAdapter.Fill(dataSet);
            }
            catch (Exception exception)
            {
                exception.ToString();
            }

            CloseConnection();

            return dataSet;
        }

        public List<string> QueryListString(string query,
            IDictionary<string, object?>? parameters = null,
            SqlParameter[]? sqlParameters = null)
        {
            List<string> list = [];
            try
            {
                Init(query, parameters, sqlParameters);
                SqlDataAdapter.SelectCommand = SqlCommand;

                using SqlDataReader reader = SqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(reader.GetString(0));
                }
            }
            catch (Exception exception)
            {
                exception.ToString();
            }

            CloseConnection();
            return list;
        }

        public List<int> QueryListInt(string query,
            IDictionary<string, object?>? parameters = null,
            SqlParameter[]? sqlParameters = null)
        {
            List<int> list = [];
            try
            {
                Init(query, parameters, sqlParameters);
                SqlDataAdapter.SelectCommand = SqlCommand;

                using SqlDataReader reader = SqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(reader.GetInt32(0));
                }
            }
            catch (Exception exception)
            {
                exception.ToString();
            }

            CloseConnection();
            return list;
        }

        public HashSet<string> QueryHashSet(string query,
            IDictionary<string, object?>? parameters = null, SqlParameter[]? sqlParameters = null)
        {
            HashSet<string> hashSet = [];
            try
            {
                Init(query, parameters, sqlParameters);
                SqlDataAdapter.SelectCommand = SqlCommand;

                using SqlDataReader reader = SqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    hashSet.Add(reader.GetString(0));
                }
            }
            catch (Exception exception)
            {
                exception.ToString();
            }

            CloseConnection();
            return hashSet;
        }

        #endregion Public Methods

        #region Stack

        public bool StackQuery(string query, int maxQueries = 1000)
        {
            StringBuilder.Append(query + " ");
            StackCounter++;

            if (StackCounter.Equals(maxQueries))
            {
                return StackFlush();
            }
            return true;
        }

        public bool StackFlush()
        {
            if (StackCounter.Equals(0))
            {
                return true;
            }
            bool result = Query(StringBuilder.ToString());
            StringBuilder.Clear();
            StackCounter = 0;
            return result;
        }

        #endregion Stack
    }
}
