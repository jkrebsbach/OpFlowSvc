using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public class PatientUtil
    {
        public static async Task<Patient> GetPatient(int patientId)
        {
            var command = string.Format("api/patient?patientId={0}", patientId);
            var patient = await WebUtility.WebRequest<Patient>(command, HttpMethod.Get);

            return patient;
        }
        public static async Task<List<Patient>> GetPatients(IEnumerable<int> patientIds)
        {
            var patientIdJson = JsonConvert.SerializeObject(patientIds);

            var command = string.Format("api/patient/array?patientIdArrayJson={0}", patientIdJson);
            var patients = await WebUtility.WebRequest<List<Patient>>(command, HttpMethod.Get);

            return patients;
        }
    }
}
