using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Specialty : IBindableEntity
    {
        public int SpecialtyID { get; set; }
        public string SpecialtyName { get; set; }
        public string SpecialtyDescription { get; set; }

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

    public class SpecialtyCardCategory
    { 
        public int SpecialtyID { get; set; }
        public int CardCategoryID { get; set; }
        public string CategoryName { get; set; }
    }

    public class SpecialtyPost
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
