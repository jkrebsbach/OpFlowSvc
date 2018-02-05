using Foundation;
using System;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS
{
    public partial class DashboardStepCell : UITableViewCell
    {
        public DashboardStepCell (IntPtr handle) : base (handle)
        {
        }

        public void UpdateCell(FlowTiming flowTiming)
        {
            lblPhase.Text = flowTiming.StepDescription;
            lblPhase.BackgroundColor = PhaseColor(flowTiming.StepDescription);

            lblStart.Text = flowTiming.StartTime.ToString("HH:mm");
            lblEnd.Text = flowTiming.EndTime.ToString("HH:mm");

            lblProcedure.Text = flowTiming.StepInstruction;
            lblDelay.Text = "";
        }


        private static UIColor PhaseColor(string phase)
        {
            switch (phase.ToLower())
            {
                case "turnover":
                case "timeout":
                    return UIColor.Blue;
                case "induction":
                case "open":
                case "critical":
                case "critical portion":
                case "work":
                    return UIColor.Yellow;
                case "close":
                case "debrief":
                    return UIColor.Red;
                default:
                    return UIColor.White;
            }
        }
    }
}