using NPOI.SS.Formula.Functions;
using SwiftExcel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpFlow.Service.DataAccess
{
    public class ExcelHelper
    {
        private static readonly object excelLock = new object();

        public static byte[] GenerateWorkbook(DataTable sourceTable)
        {
            byte[] result;
            lock (excelLock)
            {
                var fileGuid = Guid.NewGuid();
                var tempPath = Path.GetTempPath();
                //var filename = $"\\%TMP%\\{fileGuid.ToString()}";
                var filename = Path.Combine(tempPath, fileGuid.ToString());
                using (var ew = new ExcelWriter(filename))
                {
                    var rowIndex = 1;
                    for (var index = 0; index < sourceTable.Columns.Count; index++)
                    {
                        ew.Write(sourceTable.Columns[index].ColumnName, index + 1, rowIndex);
                    }

                    foreach (DataRow dataRow in sourceTable.Rows)
                    {
                        rowIndex++;

                        for (var index = 0; index < sourceTable.Columns.Count; index++)
                        {
                            var value = dataRow[index];

                            if (value == DBNull.Value)
                                continue;

                            DataType dataType;

                            if (value is String)
                                dataType = DataType.Text;
                            else if (value is DateTime)
                                dataType = DataType.Text;
                            else
                                dataType = DataType.Number;

                            ew.Write(value.ToString(), index + 1, rowIndex, dataType);
                        }
                    }

                }

                result = File.ReadAllBytes(filename);
                File.Delete(filename);
            }
            return result;
        }
        public static byte[] GenerateWorkbook<T>(IEnumerable<T> sourceData)
        {
            byte[] result;
            lock (excelLock)
            {
                var t = typeof(T);

                var fileGuid = Guid.NewGuid();
                var tempPath = Path.GetTempPath();
                //var filename = $"\\%TMP%\\{fileGuid.ToString()}";
                var filename = Path.Combine(tempPath, fileGuid.ToString());
                using (var ew = new ExcelWriter(filename))
                {
                    var rowIndex = 1;

                    var props =
                        t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty |
                                        BindingFlags.GetProperty);
                    props = props.Where(x =>
                    {
                        var setter = x.GetSetMethod();
                        return x.GetIndexParameters().Length == 0 && setter != null && setter.IsPublic;
                    }).ToArray();

                    var column = 0;
                    foreach (var propInfo in props)
                    {
                        column += 1;
                        var prop = propInfo;
                        ew.Write(prop.Name, column, rowIndex);
                    }

                    foreach (T dataRow in sourceData)
                    {
                        rowIndex++;

                        column = 0;
                        foreach (var propInfo in props)
                        {
                            column += 1;
                            var value = propInfo.GetValue(dataRow);

                            if (value == DBNull.Value)
                                continue;

                            DataType dataType;

                            if (value is String)
                                dataType = DataType.Text;
                            else if (value is DateTime)
                                dataType = DataType.Text;
                            else
                                dataType = DataType.Number;

                            ew.Write(value.ToString(), column, rowIndex, dataType);
                        }
                    }

                }

                result = File.ReadAllBytes(filename);
                File.Delete(filename);
            }
            return result;
        }
    }
}