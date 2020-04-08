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
        public List<User> Surgeons { get; set; }
        public List<Specialty> Specialties { get; set; }
        public List<CardBundle> Bundles { get; set; }
        public List<Procedure> Procedures { get; set; }
        public List<ItemMaster> Trays { get; set; }
        public List<TrayRationalization> Proposals { get; set; }
        public OpFlowLocation LocationSetup { get; set; }
    }
}
