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

            try
            {
                if (importType == CsvParser.ImportType.SPMSchedule)
                    return ParseSchedule();

                if (importType == CsvParser.ImportType.LomaLindaCard)
                    return ParseLomaLindaCard();

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<IImportData> ParseLomaLindaCard()
        {
            var result = new List<IImportData>();
            
            using (var memStream = new MemoryStream(_fileContents))
            {
                var wb = new XSSFWorkbook(memStream);
                var sheet = wb.GetSheetAt(0);

                var surgeonRegex = @"(\s\[[0-9]+\])";

                for (var rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    var sheetRow = sheet.GetRow(rowIndex);
                    if (sheetRow != null)
                    {
                        var columnA = CellValue(sheetRow.GetCell(0));
                        var columnB = CellValue(sheetRow.GetCell(1));
                        var columnC = CellValue(sheetRow.GetCell(2));
                        var columnD = CellValue(sheetRow.GetCell(3));
                        var columnE = CellValue(sheetRow.GetCell(4));
                        var columnF = CellValue(sheetRow.GetCell(5));
                        var columnG = CellValue(sheetRow.GetCell(6));

                        if (string.IsNullOrEmpty(columnC)) continue;

                        var surgeonMatch = Regex.Match(columnC, surgeonRegex);
                        if (surgeonMatch.Success) columnC = columnC.Replace(surgeonMatch.Groups[1].Value, "");

                        foreach (var item in columnF.Split('\n'))
                        {
                            if (item == "") continue;

                            var cardData = new CardImport()
                            {
                                Surgeon = columnC,
                                PreferenceCardName = columnE,
                                ItemType = "EQUIPMENT",                                
                                ItemName = item.Trim(),
                                Quantity = 1
                            };

                            result.Add(cardData);
                        }

                        foreach (var instrument in columnG.Split('\n'))
                        {
                            if (instrument == "") continue;

                            var cardData = new CardImport()
                            {
                                Surgeon = columnC,
                                PreferenceCardName = columnE,
                                ItemName = instrument.Trim(),
                                ItemType = "TRAY",
                                Quantity = 1,
                                
                            };

                            result.Add(cardData);
                        }
                    }
                }
            }
            return result;
        }

        private string CellValue(NPOI.SS.UserModel.ICell cell)
        {
            if (cell == null) return string.Empty;

            if (cell.CellType == NPOI.SS.UserModel.CellType.String)
                return cell.StringCellValue;

            else
                return cell.NumericCellValue.ToString();
        }

        private List<IImportData> ParseSchedule()
        {
            var result = new List<IImportData>();
            var caseRegex = @"Case Number: ([A-Z0-9\-]+)";

            CardlessScheduleImport schedule = null;

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
    }
}