using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Mobile
{
    public static class Helpers
    {
        public static int CalculateAge(this DateTime birthDate)
        {
            var now = DateTime.Now;
            var years = new DateTime(DateTime.Now.Subtract(birthDate).Ticks).Year - 1;

            return years;
        }
    }
}
