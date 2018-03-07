using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class PatientPosition : IBindableEntity
    {
        public int PatientPositionID { get; set; }
        public string PositionDescription { get; set; }

        public int GetID()
        {
            return PatientPositionID;
        }

        public override string ToString()
        {
            return PositionDescription;
        }
    }
}
