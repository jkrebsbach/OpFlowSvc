using NPOI.SS.Formula.Functions;
using NPOI.XSSF.UserModel;
using OpFlow.Data.Administration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            if (importType != CsvParser.ImportType.SPMSchedule)
                return result;

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
                            var abc = sheetRow.GetCell(0).StringCellValue;
                            var xyz = sheetRow.GetCell(1).StringCellValue;
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