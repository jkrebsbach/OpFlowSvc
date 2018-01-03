using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Room
    {
        public int RoomID { get; set; }
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public string RoomType { get; set; }
        public string RoomDescription { get; set; }

        public override string ToString()
        {
            return RoomDescription;
        }
    }
}
