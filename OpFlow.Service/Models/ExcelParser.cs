using NPOI.SS.Formula.Functions;
using NPOI.XSSF.UserModel;
using OpFlow.Data.Administration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace OpFlow.Service.Models
{
    public class ExcelParser
    {
        byte[] _fileContents;

        public ExcelParser(byte[] contents)
        {
            _fileContents = contents;
        }

        public List<IImportData> ParseExcel(CsvParser.ImportType importType)
        {
            var result = new List<IImportData>();
            CardlessScheduleImport schedule = null;

            if (importType != CsvParser.ImportType.SPMSchedule)
                return result;

            var caseRegex = @"Case Number: ([A-Z0-9\-]+)";

            try
            {
                using (var memStream = new MemoryStream(_fileContents))
                {
                    var wb = new XSSFWorkbook(memStream);
                    var sheet = wb.GetSheetAt(0);

                    for (var rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                    {
                        var sheetRow = sheet.GetRow(rowIndex);
                        if (sheetRow != null)
                        {
                            var columnA = sheetRow.GetCell(0).StringCellValue;

                            var caseNumber = Regex.Match(columnA, caseRegex);
                            if (caseNumber.Success)
                            {
                                schedule = new CardlessScheduleImport()
                                {
                                    CaseNbr = caseNumber.Groups[1].Value
                                };

                                result.Add(schedule);
                            }
                            else
                            {
                                if (schedule == null)
                                    continue;

                                //// old format?...
                                //schedule.Surgeon = sheetRow.GetCell(0).StringCellValue;
                                //schedule.Room = sheetRow.GetCell(1).StringCellValue;

                                //var spmDate = sheetRow.GetCell(2).StringCellValue;
                                //schedule.ScheduleDate = DateTime.Parse(spmDate);

                                //schedule.TrayList += $"{sheetRow.GetCell(3).StringCellValue},";

                                schedule.Surgeon = sheetRow.GetCell(0).StringCellValue;
                                schedule.Room = sheetRow.GetCell(1).StringCellValue;
                                var caseClass = sheetRow.GetCell(2).StringCellValue;

                                var spmDate = sheetRow.GetCell(3).StringCellValue;
                                schedule.ScheduleDate = DateTime.Parse(spmDate);

                                schedule.TrayList += $"{sheetRow.GetCell(4).StringCellValue},";
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}