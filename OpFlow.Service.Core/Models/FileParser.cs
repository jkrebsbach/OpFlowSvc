using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using OpFlow.Data;
using OpFlow.Data.Administration;
using OpFlow.Service.DataAccess;

namespace OpFlow.Service.Models
{
    public class FileParser
    {
        private string _filename;
        private byte[] _fileContents;

        public string Status;

        public FileParserRelations Relations { get; set; }
        public List<IImportData> Records { get; set; }

        public FileParser(string filename, byte[] filecontents)
        {
            _filename = filename;
            _fileContents = filecontents;
        }

        public async Task ParseFile(SqlHelper sqlHelper, int importTypeId, int locationId)
        {
            var extension = Path.GetExtension(_filename);
            switch (extension)
            {
                case ".csv":
                    var csvData = System.Text.Encoding.UTF8.GetString(_fileContents);
                    var parser = new CsvParser(csvData);
                    Records = parser.ParseCSV((CsvParser.ImportType)importTypeId);
                    Status = "good to go";
                    break;
                case ".xlsx":
                    var excelParser = new ExcelParser(_fileContents);
                    Records = excelParser.ParseExcel((CsvParser.ImportType)importTypeId);
                    Status = "good to go";
                    break;
                default:
                    Status = $"{extension} is not an accepted format.";
                    break;
            }

            Relations = new FileParserRelations();
            Relations.Roles = await sqlHelper.GetRoles(locationId);
            Relations.Specialties = await sqlHelper.GetSpecialties(locationId);
            Relations.Rooms = await sqlHelper.GetRooms(locationId);
            Relations.Surgeons = await sqlHelper.GetSurgeons(null, locationId);
        }
    }
}