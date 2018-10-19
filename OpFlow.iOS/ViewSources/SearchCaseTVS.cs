using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public class SearchCaseTVS : UITableViewSource
    {
        private readonly List<SurgerySearchModel> _surgeries;
        private readonly List<Patient> _patients;

        public event EventHandler<SurgerySearchResult> AddCaseEvent;
        public event EventHandler<SurgerySearchResult> EntitySelectionEvent;

        public SearchCaseTVS(List<SurgerySearchResult> surgeries, List<Patient> patients)
        {
            _surgeries = new List<SurgerySearchModel>();

            surgeries.ForEach(s => _surgeries.Add(new SurgerySearchModel(s)));
            _patients = patients;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var surgery = _surgeries[indexPath.Row];

            var patient = _patients.FirstOrDefault(p => p.PatientID == surgery.Surgery.PatientID);

            var cell = 
                tableView.DequeueReusableCell("CaseSearchResultCell", indexPath) as CaseSearchResultCell;

            cell?.UpdateCell(surgery, patient, this);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _surgeries.Count;
        }

        public void SelectAll(bool selectAll)
        {
            for (var index = 0; index < _surgeries.Count; index++)
                _surgeries[index].Selected = selectAll;
        }

        public class SurgerySearchModel
        {
            public bool Selected;
            public SurgerySearchResult Surgery;

            public SurgerySearchModel(SurgerySearchResult surgery)
            {
                Surgery = surgery;
            }
        }

        public void AddCaseButtonEvent(SurgerySearchResult surgery)
        {
            AddCaseEvent?.Invoke(this, surgery);
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var entity = _surgeries[indexPath.Row];

            EntitySelectionEvent?.Invoke(this, entity.Surgery);
        }
    }
}