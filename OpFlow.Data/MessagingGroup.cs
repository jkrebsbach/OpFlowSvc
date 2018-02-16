using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class MessagingGroup
    {
        public int CaseGroupID { get; set; }
        public int CommunicationUserID { get; set; }
        public string CommunicationTargetName { get; set; }
        public int SenderUserID { get; set; }
        public string LatestMessage { get; set; }
        public DateTimeOffset LatestInsertTimestamp { get; set; }
    }
}
