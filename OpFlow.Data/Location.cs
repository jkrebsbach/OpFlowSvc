using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Location
    {
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public string ProviderName { get; set; }
        public string LocationName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public int CaseToday { get; set; }
        public int CaseWeek { get; set; }
    }
}
