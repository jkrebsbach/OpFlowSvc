using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpFlow.Mobile;

namespace OpFlow.AndroidApp.Fragments
{
    public class CaseReviewFragment : CaseDetailFragment
    {
        protected override List<CaseDetailToken> GetDetailTokens()
        {
            var caseDetailTokens = Enum.GetValues(typeof(CaseDetailToken.CaseDetailEnum))
                .Cast<CaseDetailToken.CaseDetailEnum>()
                .Select(value => new CaseDetailToken(Patient, value))
                .ToList();

            return caseDetailTokens;
        }
    }
}