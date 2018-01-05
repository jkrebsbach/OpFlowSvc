using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class FlowNotification
    {
        public int FlowID { get; set; }
        public int FlowMessageID { get; set; }
        public int NotificationType { get; set; }
        public int FlowGroupID { get; set; }
        public string FlowMessage { get; set; }
    }
}

