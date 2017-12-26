using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using OpFlow.Data;

namespace OpFlow.Service.DataAccess
{
    public class SqlHelper
    {
        private static DataSet ExecuteCommand(string storedProcedure, DSAParameters[] dsParameters = null)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SecureConnection"].ConnectionString);
            var cmd = new SqlCommand(storedProcedure, conn);

            if (dsParameters != null)
                cmd.Parameters.AddRange(dsParameters);

            conn.Open();

            using (var dataAdapter = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();

                dataAdapter.Fill(ds);

                cmd.Parameters.Clear();
                conn.Close();

                return ds;
            }
        }

        public static List<Schedule> GetSchedules()
        {
            var dsSchedules = ExecuteCommand("GET_SCHEDULE");

            var result = DataTableToList<Schedule>(dsSchedules.Tables[0]);

            return result;
        }


        private static List<T> DataTableToList<T>(DataTable dt) where T : new()
        {
            return DataTableToIEnumerable<T>(dt).ToList();
        }

        private static IEnumerable<T> DataTableToIEnumerable<T>(DataTable dt) where T : new()
        {
            return new EntityReader<T>(dt);
        }
    }
}