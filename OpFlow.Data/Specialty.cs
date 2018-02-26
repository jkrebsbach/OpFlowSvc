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

        public int GetID()
        {
            return SpecialtyID;
        }

        public override string ToString()
        {
            return SpecialtyDescription;
        }
    }
}
