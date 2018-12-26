using System.Collections.Generic;
using System.Threading.Tasks;

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
}
