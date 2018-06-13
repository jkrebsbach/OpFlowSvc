using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class CardBundle : IBindableEntity
    {
        public int BundleID { get; set; }
        public int SpecialtyID { get; set; }
        public string BundleDescription { get; set; }
        public string SpecialtyDescription { get; set; }

        public int GetID()
        {
            return BundleID;
        }

        public override string ToString()
        {
            return BundleDescription;
        }

        public List<BundleProcedure> Procedures { get; set; }

        public CardBundle()
        {
            Procedures = new List<BundleProcedure>();
        }
    }

    public class BundlePost
    {
        public string BundleDescription { get; set; }   
        public int SpecialtyID { get; set; }
        public List<int> Procedures { get; set; }
    }
}
