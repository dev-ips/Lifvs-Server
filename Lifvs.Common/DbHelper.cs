using StackExchange.Profiling;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Lifvs.Common
{
    public class DbHelper
    {
        public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["LifvsDb"].ConnectionString;

        public static IDbConnection GetOpenConnection(bool mars = false)
        {
            var cs = ConnectionString;
            if (mars)
            {
                var scsb = new SqlConnectionStringBuilder(cs) { MultipleActiveResultSets = true };
                cs = scsb.ConnectionString;
            }
            var connection = new SqlConnection(cs);
            connection.Open();

            //return connection;
            // wrap the connection with a profiling connection that tracks timings 
            return new StackExchange.Profiling.Data.ProfiledDbConnection(connection, MiniProfiler.Current);
        }

        public static SqlConnection GetClosedConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
