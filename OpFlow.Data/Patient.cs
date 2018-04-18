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
        public string Initials { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public decimal BMI { get; set; }

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

    public class PatientPost
    {
        public string PatientAcctNbr { get; set; }
        public string Initials { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal BMI { get; set; }
    }
}
