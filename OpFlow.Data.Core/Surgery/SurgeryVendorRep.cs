using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class SurgeryVendorRep
    {
        public int SurgeryID { get; set; }
        public string VendorID { get; set; }
        public int VendorRepID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CellPhone { get; set; }
        public string HomeState { get; set; }
    }

    public class Vendor
    {
        public int VendorID { get; set; }
        public string VendorDescription { get; set; }
    }
}
