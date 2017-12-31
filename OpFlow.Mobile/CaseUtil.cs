using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public static class CaseUtil
    {
        public static async Task<Case> GetCase(int caseId, int locationId, int providerId)
        {
            var command = string.Format("api/case?caseId={0}&locationId={1}&providerId={2}", caseId, locationId, providerId);
            var caseResponse = await WebUtility.WebRequest<Case>(command, HttpMethod.Get);

            return caseResponse;
        }
    }
}
