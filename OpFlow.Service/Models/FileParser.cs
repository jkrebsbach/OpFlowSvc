using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using OpFlow.Data.Administration;
using OpFlow.Service.DataAccess;

namespace OpFlow.Service.Models
{
    public class FileParser
    {
        private string _filename;
        private byte[] _fileContents;

        public string Status;

        public FileParser(string filename, byte[] filecontents)
        {
            _filename = filename;
            _fileContents = filecontents;
        }

        public List<IImportData> ParseFile(int importTypeId)
        {
            
            if (Path.GetExtension(_filename) == ".csv")
            {
                var csvData = System.Text.Encoding.UTF8.GetString(_fileContents);

                var parser = new CsvParser(csvData);
                var results = parser.ParseCSV(importTypeId);

                Status = "good to go";

                return results;
            }
            else
            {
                Status = $"{Path.GetExtension(_filename)} is not an accepted format.";
            }

            return null;
        }
    }
}