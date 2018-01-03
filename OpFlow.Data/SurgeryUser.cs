using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class SurgeryUser
    {
        public int SurgeryID { get; set; }
        public int CaseID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string RoleDescription { get; set; }
        public DateTime? CheckInDate { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string WorkupReview { get; set; }
        public string UserStatus { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
    }
}