using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class SurgeryUser
    {
        public int SurgeryID { get; set; }
        public int CaseID { get; set; }
        public int CardID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public int? OrderNbr { get; set; }
        public string RoleDescription { get; set; }
        public string LocationName { get; set; }
        public DateTime? CheckInDate { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string WorkupReview { get; set; }
        public DateTime? SurgeryReview { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserTitle { get; set; }

        public DateTime? FlowUpdate { get; set; }
        public DateTime? CardUpdate { get; set; }
        public DateTime? RoomSetupUpdate { get; set; }

        public DateTime? CardRevisionDate
        {
            get
            {
                DateTime? maxDate = FlowUpdate;

                if (maxDate == null || (CardUpdate.HasValue && CardUpdate.Value > maxDate.Value))
                    maxDate = CardUpdate;
                if (maxDate == null || (RoomSetupUpdate.HasValue && RoomSetupUpdate.Value > maxDate.Value))
                    maxDate = RoomSetupUpdate;

                return maxDate;
            }
        }

        public bool SurgeryReviewObsolete
        {
            get
            {
                if (CardRevisionDate.HasValue && SurgeryReview.HasValue)
                    return CardRevisionDate.Value > SurgeryReview.Value;

                return false;
            }
        }
    }
}