using System.Collections.Generic;

namespace OpFlow.Data.Administration
{
    public interface IImportData
    {
    }

    public class FileParserRelations
    {
        // Need some blac magic for reference data of imports
        public List<Room> Rooms { get; set; }
        public List<Surgeon> Surgeons { get; set; }
    }

    public class ImportResult
    {
        public int Identity { get; set; }
        public List<string> Messages { get; set; }

        public ImportResult()
        {
            Messages = new List<string>();
        }
    }
}
