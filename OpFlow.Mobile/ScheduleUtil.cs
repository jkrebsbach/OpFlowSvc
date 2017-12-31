using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using OpFlow.Data;

namespace OpFlow.Mobile
{
    public static class ScheduleUtil
    {
        public static async Task<List<Schedule>> GetSchedule(DateTime scheduleDate)
        {
            var userId = AppSettings.CurrentSurgeon.UserID;

            var command = string.Format("api/schedule?userId={0}", userId);
            var response = await WebUtility.WebRequest<List<Schedule>>(command, HttpMethod.Get);

            return response;
        }
    }
}
