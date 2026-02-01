using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class NotificationItem
    {
        public int Id { get; set; }
        public string Message { get; set; }
    }

    public class NotificationDeviceRegistration
    {
        public string Platform { get; set; }
        public string Handle { get; set; }
        public string[] Tags { get; set; }
    }

    public class NotificationMessage
    {
        public string UserName { get; set; }
        public string Message { get; set; }
    }
}
