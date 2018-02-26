using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class CardBundle : IBindableEntity
    {
        public int BundleID { get; set; }
        public string BundleDescription { get; set; }

        public int GetID()
        {
            return BundleID;
        }

        public override string ToString()
        {
            return BundleDescription;
        }
    }
}
