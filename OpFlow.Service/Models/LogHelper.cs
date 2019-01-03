using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mindscape.Raygun4Net;

namespace OpFlow.Service.Models
{
    public static class LogHelper
    {
        private static readonly RaygunClient Client = new RaygunClient("f12C1dpwvycqBLOm2YT5rw==");

        public static void LogException(Exception ex)
        {
            Console.WriteLine(ex);
            Client.Send(ex);
        }
    }
}