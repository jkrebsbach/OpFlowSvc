using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpFlow.Data;
using OpFlow.Mobile;

namespace OpFlow.AndroidApp.Fragments
{
    public class CasePatientFragment : CaseDetailFragment
    {
        protected override async Task<List<CaseDetailToken>> GetDetailTokens()
        {
            var caseDetailTokens = Enum.GetValues(typeof(PatientDemo.DemoTypeEnum))
                .Cast<PatientDemo.DemoTypeEnum>()
                .Select(value => new CaseDetailToken(Patient, value))
                .ToList();

            return caseDetailTokens;
        }
    }
}