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
        public static async Task<List<Schedule>> GetSchedules(DateTime scheduleDate)
        {
            var response = await WebUtility.WebRequest<List<Schedule>>("api/schedule", HttpMethod.Get);

            return response;
        }

        public static async Task<Schedule> GetSchedule(int scheduleId)
        {
            var command = string.Format("api/schedule/{0}", scheduleId);
            var response = await WebUtility.WebRequest<Schedule>(command, HttpMethod.Get);

            return response;
        }
    }
}
