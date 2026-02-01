using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpFlow.Data.Administration
{
    public class CardlessScheduleImport : ScheduleBase
    {
        public string TrayList { get; set; }

        public List<string> Trays => TrayList.Split(',').ToList();
    }
}
