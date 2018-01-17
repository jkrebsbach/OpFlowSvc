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
        public static async Task<List<Card>> GetCardData(int cardId)
        {
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            var command = string.Format("api/card?cardId={0}&providerId={1}&locationId={2}", cardId, providerId, locationId);
            var response = await WebUtility.WebRequest<List<Card>>(command, HttpMethod.Get);

            return response;
        }
    }
}
