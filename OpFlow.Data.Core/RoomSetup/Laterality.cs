using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Laterality : IBindableEntity
    {
        public int LateralityID { get; set; }
        public string LateralityDescription { get; set; }

        public int GetID()
        {
            return LateralityID;
        }

        public override string ToString()
        {
            return LateralityDescription;
        }
    }
}
