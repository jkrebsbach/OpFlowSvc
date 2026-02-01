using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Sales
{
    public class SalesModel
    {
        public string SystemName { get; set; }
        public string HospitalName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ContactName { get; set; }
        public string Salesperson { get; set; }
        public int? CaseCount { get; set; }
        public int? SpdLaborRate { get; set; }
        public int? ContractDuration { get; set; }
        public int? AnnualMaintenance { get; set; }
        public int? Depreciation { get; set; }
        public int? TrayCount { get; set; }
        public int? InstrumentAvg { get; set; }
        public int? TrayYear { get; set; }
        public int? VendorYear { get; set; }
        public int? CardYear { get; set; }
        public int? ImproveYear { get; set; }
    }
}
