using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public static class SurgeonUtil
    {
        public static async Task<List<Surgeon>> GetSurgeons(DateTime scheduleDate)
        {
            var response = await WebUtility.WebRequest<List<Surgeon>>("api/surgeon", HttpMethod.Get);

            return response;
        }

        public static async Task<Surgeon> GetSurgeon(string username)
        {
            var command = string.Format("api/surgeonbyusername?username={0}", username);
            
            var response = await WebUtility.WebRequest<Surgeon>(command, HttpMethod.Get);

            return response;
        }
    }
}
