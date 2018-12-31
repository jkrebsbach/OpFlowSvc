using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Administration
{
    public class ImportType
    {
        public int ImportID { get; set; }
        public string ImportName { get; set; }
        public int NumberOfColumns { get; set; }
        public string Comments { get; set; }
    }

    public class ImportDetail
    {
        public List<ImportDefinition> ImportDefinitions{ get; set; }
        public List<ImportLog> ImportLogs { get; set; }
    }

    public class ImportDefinition
    {
        public int ImportID { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string Logic { get; set; }
    }

    public class ImportMessage
    {
        public int ImportMessageID { get; set; }
        public int ImportLogID { get; set; }
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorMRN { get; set; }
    }

    public class ImportLog
    {
        public int ImportLogID { get; set; }
        public int ImportID { get; set; }
        public int UserID { get; set; }
        public DateTime LoadDate { get; set; }
        public int RecordCount { get; set; }
        public int MessageCount { get; set; }
        public string FileName { get; set; }
        public string UserName { get; set; }
    }
}
