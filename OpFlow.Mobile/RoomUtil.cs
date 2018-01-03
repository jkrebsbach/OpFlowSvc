using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public class RoomUtil
    {
        public static async Task<List<Room>> GetRooms(int locationId)
        {
            var command = string.Format("api/room?locationId={0}", locationId);

            var response = await WebUtility.WebRequest<List<Room>>(command, HttpMethod.Get);

            return response;
        }
    }
}
