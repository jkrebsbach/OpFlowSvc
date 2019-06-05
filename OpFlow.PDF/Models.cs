using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpFlow.Data;

namespace OpFlow.PDF
{
    public class TraySummary
    {
        public TrayRationalization ProposedTray { get; set; }
        public List<TrayRationalizationItem> Instruments { get; set; }
        public List<TraySurgeryAudit> Audits {get;set;}
        public List<TraySurgeryAudit> Counts { get; set; }
        public List<SourceTraySummary> SourceTrays { get; set; }
        public List<TrayCardOverlap> Cards { get; set; }

        public List<string> Reports { get; set; }
    }
}