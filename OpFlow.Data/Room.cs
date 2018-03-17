using System;
using System.Collections.Generic;
using System.Linq;
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

    public class RoomSetup : IBindableEntity
    {
        public int RoomSetupID { get; set; }
        public int RoomTypeID { get; set; }
        public int PatientPositionID { get; set; }
        public string RoomTypeDescription { get; set; }
        public int OwnerUserID { get; set; }
        public string SetupName { get; set; }
        public string PatientPosition { get; set; }
        public string PatientAccess { get; set; }

        public int BedOrientation { get; set; }
        public string Comments { get; set; }

        public List<RoomSetupStaffPosition> StaffPositions { get; set; }
        public List<RoomSetupEquipment> SetupEquipment { get; set; }
        public List<RoomSetupItem> SetupItems { get; set; }

        public string EquipmentList()
        {
            var result = string.Join(",", SetupEquipment.Select(se => se.ItemDescription));

            return result;
        }

		public RoomSetup()
        {
            SetupEquipment = new List<RoomSetupEquipment>();
            SetupItems = new List<RoomSetupItem>();
            StaffPositions = new List<RoomSetupStaffPosition>();
        }

        public int GetID()
        {
            return RoomSetupID;
        }

        public override string ToString()
        {
            return RoomTypeDescription;
        }
    }

    public class RoomSetupStaffPosition
    {
        public int RoomSetupID { get; set; }
        public int StaffRoleID { get; set; }
        public string StaffPosition { get; set; }
    }

    public class RoomSetupEquipment
    {
        public int RoomSetupEquipmentID { get; set; }
        public int RoomSetupID { get; set; }
        public int ItemID { get; set; }
        public string ItemDescription { get; set; }
        public string EquipmentPosition { get; set; }
    }

    public class RoomSetupItem
    {
        public int RoomSetupItemID { get; set; }
        public int RoomSetupID { get; set; }
        public int ItemID { get; set; }
        public string ItemDescription { get; set; }
        public int ItemQuantity { get; set; }
        public decimal ItemCost { get; set; }
    }
}
