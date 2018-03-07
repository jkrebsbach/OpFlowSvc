using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Room : IBindableEntity
    {
        public int RoomID { get; set; }
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public int RoomTypeID { get; set; }
        public string RoomType { get; set; }
        public string RoomDescription { get; set; }

        public int GetID()
        {
            return RoomID;
        }

        public override string ToString()
        {
            return RoomDescription;
        }
    }

    public class RoomType : IBindableEntity
    {
        public int RoomTypeID { get; set; }
        public string RoomTypeDescription { get; set; }

        public int GetID()
        {
            return RoomTypeID;
        }

        public override string ToString()
        {
            return RoomTypeDescription;
        }
    }

    public class RoomSetup
    {
        public int RoomSetupID { get; set; }
        public int RoomTypeID { get; set; }
        public int OwnerUserID { get; set; }
        public string SetupName { get; set; }
        public string PatientPosition { get; set; }
        public string SurgeonPosition { get; set; }

        public int BedOrientation { get; set; }
        public string Comments { get; set; }

        public List<RoomSetupStaffPosition> StaffPositions { get; set; }
        public List<RoomSetupEquipment> SetupEquipment { get; set; }

        public RoomSetup()
        {
            SetupEquipment = new List<RoomSetupEquipment>();
        }
    }

    public class RoomSetupStaffPosition
    {
        public int StaffRoleID { get; set; }
        public string StaffPosition { get; set; }
    }

    public class RoomSetupEquipment
    {
        public int RoomSetupID { get; set; }
        public int RoomSetupEquipmentID { get; set; }
        public int ItemID { get; set; }
        public string ItemDescription { get; set; }
        public string EquipmentPosition { get; set; }
    }
}
