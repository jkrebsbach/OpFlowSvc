using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public class PatientUtil
    {
        public static async Task<List<Patient>> GetCases()
        {
            var caseResponse = await WebUtility.WebRequest<List<Patient>>("api/patient", HttpMethod.Get);

            return caseResponse;
        }

        public static async Task<Patient> GetPatient(int patientId)
        {
            var command = string.Format("api/patient/{0}", patientId);
            var patient = await WebUtility.WebRequest<Patient>(command, HttpMethod.Get);

            return patient;
        }
    }
}
