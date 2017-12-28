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

            var result = dsSchedules.Tables[0].DataTableToList<Schedule>();

            return result;
        }

        public static List<Surgeon> GetSurgeons()
        {
            var dsSchedules = ExecuteCommand("GET_SURGEONS");

            var result = dsSchedules.Tables[0].DataTableToList<Surgeon>();

            return result;
        }

        public static List<Case> GetCases()
        {
            var dsSchedules = ExecuteCommand("GET_CASES");

            var result = dsSchedules.Tables[0].DataTableToList<Case>();

            return result;
        }

        public static List<Patient> GetPatients()
        {
            var dsSchedules = ExecuteCommand("GET_PATIENTS");

            var patients = dsSchedules.Tables[0].DataTableToList<Patient>();
            var demos = dsSchedules.Tables[1].DataTableToList<PatientDemo>();

            foreach (var demo in demos)
            {
                var patient = patients.FirstOrDefault(p => p.PatientID == demo.PatientID);

                patient?.DemoData.Add(demo);
            }

            return patients;
        }
    }
}