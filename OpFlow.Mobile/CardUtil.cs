using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public abstract class CardUtil
    {
        public static async Task<List<Card>> GetCards(int? bundleId, int? procedureId)
        {
            var command = "api/card/list";
            var strDelim = "?";

            if (bundleId.HasValue)
            {
                command += $"{strDelim}bundleId={bundleId}";
                strDelim = "&";
            }
            if (procedureId.HasValue)
            {
                command += $"{strDelim}procedureId={procedureId}";
                strDelim = "&";
            }

            var response = await WebUtility.WebRequest<List<Card>>(command, HttpMethod.Get);

            return response;
        }
        public static async Task<List<Card>> GetCardData(int cardId)
        {
            var command = string.Format("api/card?cardId={0}", cardId);
            var response = await WebUtility.WebRequest<List<Card>>(command, HttpMethod.Get);

            return response;
        }
        public static async Task<List<CardItem>> GetCardItems(int cardId)
        {
            var command = string.Format("api/card/carditems?cardId={0}", cardId);
            var response = await WebUtility.WebRequest<List<CardItem>>(command, HttpMethod.Get);

            return response;
        }
        public static async Task<CardFlowRoom> GetBundleDefault(int bundleId)
        {
            var command = string.Format("api/card/bundledefault?bundleId={0}", bundleId);
            var response = await WebUtility.WebRequest<CardFlowRoom>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<CardFlowRoom> GetProcedureDefault(string cptCode)
        {
            var command = string.Format("api/card/proceduredefault?cptCode={0}", cptCode);
            var response = await WebUtility.WebRequest<CardFlowRoom>(command, HttpMethod.Get);

            return response;
        }
    }
}
