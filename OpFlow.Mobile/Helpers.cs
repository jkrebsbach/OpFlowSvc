using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Mobile
{
    public static class Helpers
    {
        public static int CalculateAge(this DateTime birthDate)
        {
            var ageTicks = DateTime.Now.Subtract(birthDate).Ticks;

            if (ageTicks <= 0)
                return 0;
            
            var years = new DateTime(ageTicks).Year - 1;

            return years;
        }
    }
}
