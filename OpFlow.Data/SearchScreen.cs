using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class SearchScreen
    {
        public List<Room> Rooms { get; set; }
        public List<RoomGroup> RoomGroups { get; set; }
        public List<User> Users { get; set; }
        public List<Specialty> Specialties { get; set; }
        public List<CardBundle> Bundles { get; set; }
        public List<Procedure> Procedures { get; set; }
    }
}
