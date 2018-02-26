using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Gender : IBindableEntity
    {
        public int GenderID { get; set; }
        public string GenderName { get; set; }

        public int GetID()
        {
            return GenderID;
        }

        public override string ToString()
        {
            return GenderName;
        }
    }
}
