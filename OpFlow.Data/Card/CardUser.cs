using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class CardUser
    {
        public int CaseID { get; set; }
        public int CardID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public int? CoSurgeonOrderNbr { get; set; }
        public string RoleDescription { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserTitle { get; set; }
    }
}
