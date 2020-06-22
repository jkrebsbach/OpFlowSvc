using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
    }
}