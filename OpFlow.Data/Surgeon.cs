using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Surgeon : IBindableEntity
    {
        public int UserID { get; set; }
        public int SpecialtyID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string Initials { get; set; }

        public int GetID()
        {
            return UserID;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1} {2}", LastName, FirstName, Title);
        }

    }
}
