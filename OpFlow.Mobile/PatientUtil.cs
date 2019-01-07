using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public abstract class PatientUtil
    {
        public static async Task<Patient> GetPatient(int patientId)
        {
            var command = string.Format("api/patient?patientId={0}", patientId);
            var patient = await WebUtility.WebRequest<Patient>(command, HttpMethod.Get);

            return patient;
        }
        public static async Task<List<Patient>> GetPatients(List<int> patientIds)
        {
            var command = string.Format("api/patient/array");
            var body = new Dictionary<string, List<int>>();
            body["PatientArray"] = patientIds;

            var patients = await WebUtility.SendBodyRequest<List<Patient>>(command, body, HttpMethod.Post);

            return patients;
        }
    }
}
