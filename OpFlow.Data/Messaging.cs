using System;
namespace OpFlow.Data
{
    public class Messaging
    {
        public int MessageID { get; set; }
        public string Message { get; set; }
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public int Type { get; set; }
        public int UserID { get; set; }
    }
}
