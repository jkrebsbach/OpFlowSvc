using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OpFlow.Data
{
    public class User
    {
        public int UserID { get; set; }
        public RoleEnum RoleID { get; set; }
        public int? SpecialtyID { get; set; }
        public int? MasterSpecialtyID { get; set; }
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public string Initials { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string CellPhone { get; set; }
        public string Specialty { get; set; }
        public string RoleName { get; set; }
        public string RoleType { get; set; }
        public string LocationName { get; set; }
        public string UserSettings { get; set; }
        public bool PHILocation { get; set; }
        public bool Vendor { get; set; }

        public string DeriveInitials()
        {
            if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
            {
                return $"{FirstName[0]}{LastName[0]}";
            }

            if (!string.IsNullOrEmpty(LastName))
                return LastName;

            return FirstName;
        }
    }

    public class InternalUser : User
    {
        public int VendorID { get; set; } 
    }

    public class UserPost
    {
        public int RoleID { get; set; }
        public int? SpecialtyID { get; set; }
        public string Initials { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string CellPhone { get; set; }
        public string Password { get; set; }
    }

    public class UserEdit
    {
        public RoleEnum RoleID { get; set; }
        public int? SpecialtyID { get; set; }
        public string Initials { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string CellPhone { get; set; }
    }

    public class UserSecurity
    {
        public int UserID { get; set; }
        public string UserSettings { get; set; }
        public bool Vendor { get; set; }
        public Guid UserAuthID { get; set; }
        public string CaseDatabaseName { get; set; }
        public string SecureDatabaseName { get; set; }
        public string RoleType { get; set; }
        public string BlobKey { get; set; }
        public string BlobContainer { get; set; }
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public int? VendorLocationID { get; set; }

        public int SelectedLocation => VendorLocationID ?? LocationID;
    }

    public class Role : IBindableEntity
    {
        public int RoleID { get; set; }
        public string RoleDescription { get; set; }

        public int GetID() {
            return RoleID;
        }

        public override string ToString()
        {
            return RoleDescription;
        }
    }

    public enum RoleEnum
    {
        Surgeon = 1,
        Circulator = 2,
        ScrubTech = 3,
        CRNA = 4,
        PA = 5,
        FrontDesk = 7,
        Scheduler = 8,
        Administration = 9,
        Representative = 10,
        Anesthesiologist = 11,
    }
}
