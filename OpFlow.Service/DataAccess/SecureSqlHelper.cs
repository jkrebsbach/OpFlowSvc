using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using OpFlow.Data;

namespace OpFlow.Service.DataAccess
{
    public static class SecureSqlHelper 
    {
        private static async Task<DataSet> ExecuteCommandAsync(string storedProcedure, string secureDatabase, SqlParameter[] dsParameters = null)
        {
            var secureConnString =
                string.Format(ConfigurationManager.ConnectionStrings["SecureConnection"].ConnectionString,
                    secureDatabase);

            var conn = new SqlConnection(secureConnString);

            var cmd = new SqlCommand(storedProcedure, conn) { CommandType = CommandType.StoredProcedure };

            if (dsParameters != null)
                cmd.Parameters.AddRange(dsParameters);

            await conn.OpenAsync();

            using (var dataAdapter = new SqlDataAdapter(cmd))
            {
                var ds = new DataSet();

                dataAdapter.Fill(ds);

                cmd.Parameters.Clear();
                conn.Close();

                return ds;
            }
        }

        private static async Task<int> ExecuteNonQuery(string storedProcedure, string secureDatabase, SqlParameter[] dsParameters = null)
        {
            var secureConnString =
                string.Format(ConfigurationManager.ConnectionStrings["SecureConnection"].ConnectionString,
                    secureDatabase);

            var conn = new SqlConnection(secureConnString);

            var cmd = new SqlCommand(storedProcedure, conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddRange(dsParameters);

            await conn.OpenAsync();

            var result = cmd.ExecuteNonQuery();

            cmd.Parameters.Clear();
            conn.Close();

            return result;
        }

        public static async Task<Patient> GetPatient(int patientId, string databaseName)
        {
            var parameters = new[]
            {
                new SqlParameter("patient_id", patientId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetPatient", databaseName, parameters);

            var patients = dsSchedules.Tables[0].DataTableToList<Patient>();
            var demos = dsSchedules.Tables[1].DataTableToList<PatientDemo>();

            foreach (var demo in demos)
            {
                var patient = patients.FirstOrDefault(p => p.PatientID == demo.PatientID);

                patient?.DemoData.Add(demo);
            }

            return patients.FirstOrDefault();
        }

        public static async Task<int> CreatePatient(string ptAcctNbr, string initials, DateTime birthDate, string gender, 
            string firstName, string lastName, decimal bmi, string databaseName)
        {
            var dsParameters = new[]
            {
                new SqlParameter("pt_acct_nbr", ptAcctNbr),
                new SqlParameter("initials", initials),
                new SqlParameter("birth_date", birthDate),
                new SqlParameter("last_name", lastName),
                new SqlParameter("first_name", firstName),
                new SqlParameter("gender", gender),
                new SqlParameter("bmi", bmi)
            };
            var insert = await ExecuteCommandAsync("NewPatient", databaseName, dsParameters);

            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }
    }
}