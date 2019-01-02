using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mindscape.Raygun4Net;

namespace OpFlow.Service.Models
{
    public static class LogHelper
    {
        private static RaygunClient _client;

        public static void LogException(Exception ex)
        {
            Console.WriteLine(ex);

            if (_client == null)
                _client = new RaygunClient("f12C1dpwvycqBLOm2YT5rw==");

            _client.Send(ex);
        }
    }
}