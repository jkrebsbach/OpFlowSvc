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
        public static async Task<Case> GetCase(int caseId)
        {
            var command = string.Format("api/case/{0}", caseId);
            var caseResponse = await WebUtility.WebRequest<Case>(command, HttpMethod.Get);

            if (caseResponse != null)
            {
                command = string.Format("api/card/{0}", caseResponse.CardID);
                caseResponse.Card = await WebUtility.WebRequest<Card>(command, HttpMethod.Get);
            }

            return caseResponse;
        }
    }
}
