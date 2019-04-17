using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using OpFlow.Data;
using OpFlow.Data.Administration;

namespace OpFlow.Service.DataAccess
{
    public class SecureSqlHelper : SqlBase
    {
        public SecureSqlHelper(string secureDatabase) : base(secureDatabase)
        {
        }

        public async Task<Patient> GetPatient(int patientId, 
            int userId, string userFirstName, string userLastName, int userRole)
        {
            var parameters = new[]
            {
                new SqlParameter("patient_id", patientId),
                new SqlParameter("user_id", userId),
                new SqlParameter("user_first_name", userFirstName),
                new SqlParameter("user_last_name", userLastName),
                new SqlParameter("user_role", userRole)
            };
            var dsSchedules = await ExecuteCommandAsync("GetPatient", parameters);

            var patients = dsSchedules.Tables[0].DataTableToList<Patient>();
            var demos = dsSchedules.Tables[1].DataTableToList<PatientDemo>();

            foreach (var demo in demos)
            {
                var patient = patients.FirstOrDefault(p => p.PatientID == demo.PatientID);

                patient?.DemoData.Add(demo);
            }

            return patients.FirstOrDefault();
        }

        public async Task<int> CreatePatient(string ptAcctNbr, DateTime? birthDate, string gender, 
            string firstName, string lastName, string middleInitial, decimal? bmi, 
            int userId, string userFirstName, string userLastName, int? userRoleId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("pt_acct_nbr", ptAcctNbr ?? (object)DBNull.Value),
                new SqlParameter("birth_date", birthDate ?? (object)DBNull.Value),
                new SqlParameter("last_name", lastName ?? (object)DBNull.Value),
                new SqlParameter("first_name", firstName ?? (object)DBNull.Value),
                new SqlParameter("middle_initial", middleInitial ?? (object)DBNull.Value),
                new SqlParameter("gender", gender ?? (object)DBNull.Value),
                new SqlParameter("bmi", bmi ?? (object)DBNull.Value),
                new SqlParameter("user_id", userId),
                new SqlParameter("user_first_name", userFirstName ?? (object)DBNull.Value),
                new SqlParameter("user_last_name", userLastName ?? (object)DBNull.Value),
                new SqlParameter("user_role", userRoleId ?? (object)DBNull.Value),
            };
            var insert = await ExecuteCommandAsync("NewPatient", dsParameters);

            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public async Task<int?> InsertStagingData(IImportData sourceData,
            int userId, string userFirstName, string userLastName, int? userRoleId)
        {
            if (sourceData is ScheduleImport schedule)
            {
                var dsParameters = new[]
                {
                    new SqlParameter("pt_acct_nbr", schedule.MRN ?? (object)DBNull.Value),
                    new SqlParameter("last_name", schedule.PatientLastName ?? (object)DBNull.Value),
                    new SqlParameter("first_name", schedule.PatientFirstName ?? (object)DBNull.Value),
                    new SqlParameter("middle_initial", schedule.PatientMInit ?? (object)DBNull.Value),
                    new SqlParameter("birth_date", schedule.DateOfBirth ?? (object)DBNull.Value),
                    new SqlParameter("bmi", schedule.BMI ?? (object)DBNull.Value),
                    new SqlParameter("gender", schedule.Gender ?? (object)DBNull.Value),
                    new SqlParameter("medical_history", schedule.MedicalHistory ?? (object)DBNull.Value),
                    new SqlParameter("risk_factors", schedule.RiskFactors ?? (object)DBNull.Value),
                    new SqlParameter("medications", schedule.Medications ?? (object)DBNull.Value),
                    new SqlParameter("allergies", schedule.Allergies ?? (object)DBNull.Value),
                    new SqlParameter("notes", schedule.Notes ?? (object)DBNull.Value),
                    new SqlParameter("user_id", userId),
                    new SqlParameter("user_first_name", userFirstName ?? (object)DBNull.Value),
                    new SqlParameter("user_last_name", userLastName ?? (object)DBNull.Value),
                    new SqlParameter("user_role", userRoleId ?? (object)DBNull.Value),
                };
                var insert = await ExecuteCommandAsync("NewPatient_Staging", dsParameters);

                var result = insert.Tables[0].DataTableToList<InsertionResult>();

                return result.FirstOrDefault()?.Identifier ?? -1;
            }

            return null;
        }

        public async Task<int> CleanupPatients(List<Patient> ignorePatients)
        {
            var patientSummary = GetPatientSummary(ignorePatients);

            var dsParameters = new[]
            {
                new SqlParameter("ignore_patients", patientSummary)
            };
            var result = await ExecuteNonQueryAsync("CleanupPatients", dsParameters);

            return result;
        }

        private string GetPatientSummary(List<Patient> patients)
        {
            if (!patients.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var patient in patients)
            {
                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, patient.PatientID);
            }

            return table.OuterXml;
        }
    }
}