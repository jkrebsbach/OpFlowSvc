using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Patient
    {
        public int PatientID { get; set; }
        public string PatientAcctNbr { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleInitial { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public decimal BMI { get; set; }

        public string Initials
        {
            get
            {
                var finit = string.IsNullOrEmpty(FirstName) ? "" : FirstName.Substring(0, 1);
                var minit = string.IsNullOrEmpty(MiddleInitial) ? "" : MiddleInitial.Substring(0, 1);
                var linit = string.IsNullOrEmpty(LastName) ? "" : LastName.Substring(0, 1);

                return $"{finit}{minit}{linit}";
            }
        }

        public int PatientAge
        {
            get
            {
                var ageTicks = DateTime.Now.Subtract(BirthDate).Ticks;

                if (ageTicks <= 0)
                    return 0;

                var years = new DateTime(ageTicks).Year - 1;

                return years;
            }
        }

        public List<PatientDemo> DemoData { get; set; }

        public Patient()
        {
            DemoData = new List<PatientDemo>();
        }
    }

    public class PatientArrayPost
    {
        public List<int> PatientArray { get; set; }   
    }

    public class PatientPost
    {
        public string PatientAcctNbr { get; set; }
        public string MiddleInitial { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal BMI { get; set; }
    }
}
