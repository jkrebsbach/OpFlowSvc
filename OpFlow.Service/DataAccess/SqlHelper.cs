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
    public static class SqlHelper 
    {
        private static DataSet ExecuteCommand(string storedProcedure, SqlParameter[] dsParameters = null)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["OpFlowConnection"].ConnectionString);
            var cmd = new SqlCommand(storedProcedure, conn) {CommandType = CommandType.StoredProcedure};

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

        public static List<Card> GetCardItems(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardItems", parameters);

            var result =  dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static List<Surgeon> GetSurgeons(string username)
        {
            var dsParameters = new[]
            {
                new SqlParameter("email", username),
            };
            var dsSchedules = ExecuteCommand("GetSurgeon", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgeon>();

            return result;
        }

        public static List<Schedule> GetSchedules(int userID)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userID)
            };
            var dsSchedules = ExecuteCommand("GetSurgeonCases", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Schedule>();

            foreach (var schedule in result)
            {
                schedule.Patient = SecureSqlHelper.GetPatient(schedule.PatientID);
            }

            return result;
        }

        public static Case GetCase(int caseId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("case_id", caseId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCase", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Case>();

            return result.FirstOrDefault();
        }
    }
}