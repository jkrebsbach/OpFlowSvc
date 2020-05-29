using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class SurgeonProfilePost
    {
        public List<int> SurgeonID { get; set; }
    }

    public class VendorCreateProviderPost
    {
        public string Provider { get; set; }
    }

    public class VendorCreateLocationPost
    {
        public int ProviderID { get; set; }
        public string Location { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}
