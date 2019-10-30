using System;
namespace OpFlow.Data
{
    public class Messaging
    {
        public int MessageID { get; set; }
        public int SurgeryID { get; set; }
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public int Type { get; set; }
        public int RecipientUserID { get; set; }

        public RoleEnum SenderRoleID { get; set; }
        public int SenderUserID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string UserName
        {
            get
            {
                if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
                {
                    return $"{FirstName[0]}{LastName[0]}";
                }

                if (!string.IsNullOrEmpty(LastName))
                    return LastName;

                return FirstName;
            }
        }

        public string Message { get; set; }
        public DateTimeOffset InsertTimestamp { get; set; }
        public bool MessageAcknowledged { get; set; }
    }

    public class MessagePost
    {
        public string Message { get; set; }
    }
}
