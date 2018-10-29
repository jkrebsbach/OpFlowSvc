using System;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;

using Foundation;
using UIKit;
using OpFlow.iOS.ViewSources;

namespace OpFlow.iOS
{
    public partial class OpenCaseCell :  ScheduleTableCell
    {
        private SurgeryTVS _surgeryTvs;
        private SurgerySearchResult _surgery;

        public OpenCaseCell (IntPtr handle) : base (handle)
        {
        }

        partial void btnDebrief_Click(UIButton sender)
        {
            _surgeryTvs?.NavigationButtonEvent(_surgery);
        }

        internal override void UpdateCell(SurgerySearchResult surgery, Patient patient, SurgeryTVS surgeryTVS)
        {
            _surgeryTvs = surgeryTVS;
            _surgery = surgery;

            var patientName = $"{patient?.LastName} {patient?.FirstName}".Trim();
            if (patientName == string.Empty)
                patientName = patient?.LastName ?? string.Empty;
            if (patientName == string.Empty)
                patientName = "UNK";

            lblLocation.Text = surgery.RoomDescription;
            lblPatientInfo.Text = string.Format("{0} {1}", patient?.BirthDate.CalculateAge(), patient?.Gender);

            lblPatientName.Text = patientName;
            lblProcedure.Text = surgery.ProcedureDescription;
            lblSurgeryTime.Text = surgery.ScheduleTime.ToString(@"hh\:mm");

            lblFlowStep.Text = surgery.FlowStepDescription;

        }
    }
}