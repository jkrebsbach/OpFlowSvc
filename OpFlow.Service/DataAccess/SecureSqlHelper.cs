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
    public static class SecureSqlHelper 
    {
        private static DataSet ExecuteCommand(string storedProcedure, string secureDatabase, SqlParameter[] dsParameters = null)
        {
            var secureConnString =
                string.Format(ConfigurationManager.ConnectionStrings["SecureConnection"].ConnectionString,
                    secureDatabase);

            var conn = new SqlConnection(secureConnString);

            var cmd = new SqlCommand(storedProcedure, conn) { CommandType = CommandType.StoredProcedure };

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

        private static int ExecuteNonQuery(string storedProcedure, string secureDatabase, SqlParameter[] dsParameters = null)
        {
            var secureConnString =
                string.Format(ConfigurationManager.ConnectionStrings["SecureConnection"].ConnectionString,
                    secureDatabase);

            var conn = new SqlConnection(secureConnString);

            var cmd = new SqlCommand(storedProcedure, conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddRange(dsParameters);

            conn.Open();

            var result = -1;

            using (var dataAdapter = new SqlDataAdapter(cmd))
            {
                result = cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();
                conn.Close();

                return result;
            }
        }

        public static Patient GetPatient(int patientId, string databaseName)
        {
            var parameters = new[]
            {
                new SqlParameter("patient_id", patientId)
            };
            var dsSchedules = ExecuteCommand("GetPatient", databaseName, parameters);

            var patients = dsSchedules.Tables[0].DataTableToList<Patient>();
            var demos = dsSchedules.Tables[1].DataTableToList<PatientDemo>();

            foreach (var demo in demos)
            {
                var patient = patients.FirstOrDefault(p => p.PatientID == demo.PatientID);

                if (patient == null)
                    continue;

                patient.DemoData.Add(demo);
            }

            return patients.FirstOrDefault();
        }

        public static int CreatePatient(PatientPost patient, string databaseName)
        {
            var dsParameters = new[]
            {
                new SqlParameter("initials", patient.Initials),
                new SqlParameter("birth_date", patient.BirthDate),
                new SqlParameter("gender", patient.Gender)
            };
            return ExecuteNonQuery("NewPatient", databaseName, dsParameters);
        }
    }
}