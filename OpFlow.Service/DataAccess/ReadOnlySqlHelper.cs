using Newtonsoft.Json;
using OpFlow.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OpFlow.Service.DataAccess
{
    public class ReadOnlySqlHelper : SqlHelper
    {
        public ReadOnlySqlHelper() : base("CommonConnection")
        {
        }

    }
}