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
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTimeOffset InsertTimestamp { get; set; }
    }

    public class MessagePost
    {
        public string Message { get; set; }
    }
}
