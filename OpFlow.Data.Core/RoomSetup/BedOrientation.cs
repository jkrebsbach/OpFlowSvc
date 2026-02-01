using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class BedOrientation : IBindableEntity
    {
        public int BedOrientationID { get; set; }
        public string BedOrientationDescription { get; set; }

        public int GetID()
        {
            return BedOrientationID;
        }

        public override string ToString()
        {
            return BedOrientationDescription;
        }
    }
}
