using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace OpFlow.Service.DataAccess
{
    public abstract class SqlBase
    {
        private string _connString;
        private bool _valid;
        public bool Valid => _valid;

        public SqlBase(string sqlDatabase)
        {
            if (ConfigurationManager.ConnectionStrings[sqlDatabase] == null)
            {
                _valid = false;
            }
            else
            {
                _valid = true;
                _connString = ConfigurationManager.ConnectionStrings[sqlDatabase].ConnectionString;
            }
        }


        internal async Task<DataSet> ExecuteCommandAsync(string storedProcedure, SqlParameter[] dsParameters = null)
         {
            try
            {
                using (var conn =
                    new SqlConnection(_connString))
                using (var cmd = new SqlCommand(storedProcedure, conn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.CommandTimeout = 60 * 3;
                    cmd.Parameters.AddRange(dsParameters);

                    await conn.OpenAsync();

                    using (var dataAdapter = new SqlDataAdapter(cmd))
                    {
                        var ds = new DataSet();

                        await Task.Run(() => dataAdapter.Fill(ds));

                        cmd.Parameters.Clear();
                        conn.Close();

                        return ds;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        internal async Task<int> ExecuteNonQueryAsync(string storedProcedure, SqlParameter[] dsParameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var conn = new SqlConnection(_connString))
            using (var cmd = new SqlCommand(storedProcedure, conn) { CommandType = commandType })
            {
                cmd.CommandTimeout = 60 * 3;
                cmd.Parameters.AddRange(dsParameters);

                await conn.OpenAsync();

                var result = await cmd.ExecuteNonQueryAsync();

                cmd.Parameters.Clear();
                conn.Close();

                return result;
            }
        }

        protected static void AddColumn(XmlDocument doc, XmlElement row, object value)
        {
            var col = doc.CreateElement("col");
            col.InnerText = $"{value}";
            row.AppendChild(col);
        }
    }
}