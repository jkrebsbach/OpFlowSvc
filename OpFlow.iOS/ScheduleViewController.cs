using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class ScheduleViewController : UIViewController, IUITableViewDelegate, INavigationTargetDelegate
    {
        private DateTime _selectedDate;

        public ScheduleViewController(IntPtr handle) : base(handle)
        {
            _selectedDate = DateTime.Today;
        }

        public INavigationDelegate NavigationDelegate { get; set; }
        
        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "SCHEDULEVIEW";
            SetupNavBar();

            ScheduleTableView.RowHeight = 120f;
            ScheduleTableView.EstimatedRowHeight = 40f;

            // rotate arrow 180deg / pi radians
            btnPrev.Transform = CGAffineTransform.MakeRotation((float)Math.PI);

            InitializeButton(btnSunday);
            InitializeButton(btnMonday);
            InitializeButton(btnTuesday);
            InitializeButton(btnWednesday);
            InitializeButton(btnThursday);
            InitializeButton(btnFriday);
            InitializeButton(btnSaturday);

            pickerRoom.Hidden = true;

            switchSurgeon.ValueChanged += async delegate
            {
                pickerRoom.Hidden = switchSurgeon.On;

                await LoadSchedule();
            };

            var rooms = await AppSettings.RoomList(AppSettings.CurrentUser.LocationID);
            pickerRoom.Model = new RoomPickerModel(rooms);

            await UpdateDateStrings();
            ScheduleTableView.ReloadData();
        }

        private void SetupNavBar()
        {
            NavigationItem.SetHidesBackButton(true, false);
        }

        private void InitializeButton(UIButton button)
        {
            button.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
            button.TitleLabel.TextAlignment = UITextAlignment.Center;
            button.TitleLabel.Lines = 0;

            button.Layer.BorderWidth = 1.0f;
            button.Layer.BorderColor = UIColor.Black.CGColor;

            button.TouchUpInside += async delegate(object sender, EventArgs e)
            {
                var dayOfWeek = 0;
                var clickButton = (UIButton)sender;

                if (clickButton.Tag == btnSunday.Tag)
                       dayOfWeek = 0;
                else if (clickButton.Tag == btnMonday.Tag)
                        dayOfWeek = 1;
                else if (clickButton.Tag == btnTuesday.Tag)
                        dayOfWeek = 2;
                else if (clickButton.Tag == btnWednesday.Tag)
                        dayOfWeek = 3;
                else if (clickButton.Tag == btnThursday.Tag)
                        dayOfWeek = 4;
                else if (clickButton.Tag == btnFriday.Tag)
                        dayOfWeek = 5;
                else if (clickButton.Tag == btnSaturday.Tag)
                        dayOfWeek = 6;

                await ChangeDay(dayOfWeek);
            };
        }

        async partial void btnPrevClicked(UIButton sender)
        {
            await ChangeWeek(-1);
        }

        async partial void btnNextClicked(UIButton sender)
        {
            await ChangeWeek(+1);
        }

        private async Task ChangeWeek(int weekDelta)
        {
            _selectedDate = _selectedDate.AddDays(7 * weekDelta);

            await UpdateDateStrings();
        }

        private async Task ChangeDay(int dayOfWeek)
        {
            var weekDelta = dayOfWeek - (int)_selectedDate.DayOfWeek;
            _selectedDate = _selectedDate.AddDays(weekDelta);

            await UpdateDateStrings();
        }

        [Export("tableView:heightForRowAtIndexPath:")]
        public nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 150f;
        }

        private async Task UpdateDateStrings()
        {
            lblCurrentWeek.Text = _selectedDate.ToString("MMMM yyyy");

            var dateSunday = _selectedDate.AddDays(-1 * (int)_selectedDate.DayOfWeek);
            btnSunday.SetTitle( $"S\n{dateSunday:dd}", UIControlState.Normal);
            btnMonday.SetTitle($"M\n{dateSunday.AddDays(1):dd}", UIControlState.Normal);
            btnTuesday.SetTitle($"T\n{dateSunday.AddDays(2):dd}", UIControlState.Normal);
            btnWednesday.SetTitle($"W\n{dateSunday.AddDays(3):dd}", UIControlState.Normal);
            btnThursday.SetTitle($"T\n{dateSunday.AddDays(4):dd}", UIControlState.Normal);
            btnFriday.SetTitle($"F\n{dateSunday.AddDays(5):dd}", UIControlState.Normal);
            btnSaturday.SetTitle($"S\n{dateSunday.AddDays(6):dd}", UIControlState.Normal);

            btnSunday.BackgroundColor = UIColor.White;
            btnMonday.BackgroundColor= UIColor.White;
            btnTuesday.BackgroundColor= UIColor.White;
            btnWednesday.BackgroundColor= UIColor.White;
            btnThursday.BackgroundColor= UIColor.White;
            btnFriday.BackgroundColor= UIColor.White;
            btnSaturday.BackgroundColor= UIColor.White;

            UIButton currentDate = null;
            switch (_selectedDate.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    currentDate = btnSunday;
                    break;
                case DayOfWeek.Monday:
                    currentDate = btnMonday;
                    break;
                case DayOfWeek.Tuesday:
                    currentDate = btnTuesday;
                    break;
                case DayOfWeek.Wednesday:
                    currentDate = btnWednesday;
                    break;
                case DayOfWeek.Thursday:
                    currentDate = btnThursday;
                    break;
                case DayOfWeek.Friday:
                    currentDate = btnFriday;
                    break;
                case DayOfWeek.Saturday:
                    currentDate = btnSaturday;
                    break;
            }

            if (currentDate != null)
                currentDate.BackgroundColor = UIColor.Yellow;

            await LoadSchedule();
        }

        private async Task LoadSchedule()
        {
            var schedule = await SurgeryUtil.GetSurgeryUserSchedule(DateTime.Today);
            var patientIds = schedule.Select(s => s.PatientID).ToList();
            var patients = await PatientUtil.GetPatients(patientIds);

            var surgeryPatients = new Dictionary<int, Patient>();
            schedule.ForEach(s => surgeryPatients[s.SurgeryID] = patients.FirstOrDefault(p => p.PatientID == s.PatientID));

            var surgeryTableViewSource = new SurgeryTVS(schedule, surgeryPatients);
            surgeryTableViewSource.SurgerySelectionEvent += SelectSurgery;

            ScheduleTableView.Source = surgeryTableViewSource;
        }

        private void SelectSurgery(object sender, Surgery surgery)
        {
            NavigationDelegate?.Navigate(AppSettings.FragmentEnum.Schedule, surgery);
        }
    }
}