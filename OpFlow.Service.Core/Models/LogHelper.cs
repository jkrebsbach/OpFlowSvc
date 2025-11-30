using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpFlow.Service.Models
{
    public static class LogHelper
    {

        public static void LogException(Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}