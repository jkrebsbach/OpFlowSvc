using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace OpFlow.Service.DataAccess
{
    public static class DatabaseHelper
    {
        internal static List<T> DataTableToList<T>(this DataTable dt ) where T : new()
        {
            return DataTableToIEnumerable<T>(dt).ToList();
        }

        internal static IEnumerable<T> DataTableToIEnumerable<T>(this DataTable dt) where T : new()
        {
            return new EntityReader<T>(dt);
        }
    }
}