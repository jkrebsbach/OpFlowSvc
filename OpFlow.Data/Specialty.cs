using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpFlow.Data
{
    public class Specialty : IBindableEntity
    {
        public int SpecialtyID { get; set; }
        public int? MasterSpecialtyID { get; set; }
        public string SpecialtyName { get; set; }
        public string SpecialtyDescription { get; set; }
        public string MasterSpecialty { get; set; }

        public List<SpecialtyCardCategory> CardCategories { get; set; }

        public int GetID()
        {
            return SpecialtyID;
        }

        public override string ToString()
        {
            return SpecialtyDescription;
        }
    }

    public class MasterSpecialty : Specialty
    {
        public List<Specialty> SpecialtyList { get; set; }
        public bool IsAssigned => (SpecialtyList?.Any() == true);

        public string SpecialtyAlias => SpecialtyList?.FirstOrDefault()?.SpecialtyName;
        public int? SpecialtyAliasID => SpecialtyList?.FirstOrDefault()?.SpecialtyID;
    }

    public class SpecialtyCardCategory
    { 
        public int SpecialtyID { get; set; }
        public int CardCategoryID { get; set; }
        public string CategoryName { get; set; }
    }

    public class SpecialtyPost
    {
        public List<Specialty> Specialties { get; set; }
    }
}
