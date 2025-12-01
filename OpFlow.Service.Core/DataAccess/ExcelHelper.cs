using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
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

        public static byte[] GenerateWorkbook(List<DataTable> sourceTables)
        {
            byte[] result;
            lock (excelLock)
            {
                var fileGuid = Guid.NewGuid();
                var tempPath = Path.GetTempPath();
                //var filename = $"\\%TMP%\\{fileGuid.ToString()}";
                var filename = Path.Combine(tempPath, fileGuid.ToString());
                using (var workbook = SpreadsheetDocument.Create(filename, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = workbook.AddWorkbookPart();
                    workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
                    workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

                    uint sheetId = 1;

                    foreach (DataTable table in sourceTables)
                    {
                        var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                        var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                        sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                        DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                        string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                        if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                        {
                            sheetId =
                                sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                        }

                        DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
                        sheets.Append(sheet);

                        DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                        List<String> columns = new List<string>();
                        foreach (DataColumn column in table.Columns)
                        {
                            columns.Add(column.ColumnName);

                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.ColumnName);
                            headerRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(headerRow);

                        foreach (DataRow dsrow in table.Rows)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                            foreach (String col in columns)
                            {
                                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                                cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                                if (dsrow[col].GetType() == typeof(decimal) || dsrow[col].GetType() == typeof(int))
                                    cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;

                                if (dsrow[col].GetType() == typeof(decimal))
                                    cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue((decimal)dsrow[col]);
                                else if (dsrow[col].GetType() == typeof(int))
                                    cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue((int)dsrow[col]);
                                else
                                    cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(dsrow[col].ToString());

                                newRow.AppendChild(cell);
                            }

                            sheetData.AppendChild(newRow);
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