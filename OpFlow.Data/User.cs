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
        public string Initials { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
    }

    public class UserSecurity
    {
        public int UserID { get; set; }
        public string DatabaseName { get; set; }
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
    }

    public enum RoleEnum
    {
        Surgeon = 1,
        Circulator = 2,
        ScrubTech = 3,
        CRNA = 4,
        PA = 5,
        FrontDesk = 7,
        Scheduler = 8,
        Administration = 9,
        Representative = 10,
        Anesthesiologist = 11,
    }
}
