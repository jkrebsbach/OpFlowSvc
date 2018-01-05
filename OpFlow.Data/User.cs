using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OpFlow.Data
{
    public class User
    {
        public int UserID { get; set; }
        public RoleEnum RoleID { get; set; }
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public enum RoleEnum
    {
        Surgeon = 1,
        Coordinator = 2,
        Anesthetist = 3,
        Specialist = 4,
        Office = 5,
        Billing = 6
    }
}
