using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace OpFlow.AndroidApp.Fragments
{
    public abstract class WeekDayPickerFragment : OpFlowFragmentBase
    {
        private TextView txtCurrentWeek;
        
        private TextView txtWeekSunday;
        private TextView txtWeekMonday;
        private TextView txtWeekTuesday;
        private TextView txtWeekWednesday;
        private TextView txtWeekThursday;
        private TextView txtWeekFriday;
        private TextView txtWeekSaturday;

        public DateTime SelectedDate { get; private set; }

        public void InitializePicker(View rootView)
        {
            SelectedDate = DateTime.Today;
            
            var ivWeekBack = rootView.FindViewById<ImageView>(Resource.Id.ivWeekBack);
            txtCurrentWeek = rootView.FindViewById<TextView>(Resource.Id.txtCurrentWeek);
            var ivWeekForward = rootView.FindViewById<ImageView>(Resource.Id.ivWeekForward);
            
            txtWeekSunday = rootView.FindViewById<TextView>(Resource.Id.txtWeekSunday);
            txtWeekMonday = rootView.FindViewById<TextView>(Resource.Id.txtWeekMonday);
            txtWeekTuesday = rootView.FindViewById<TextView>(Resource.Id.txtWeekTuesday);
            txtWeekWednesday = rootView.FindViewById<TextView>(Resource.Id.txtWeekWednesday);
            txtWeekThursday = rootView.FindViewById<TextView>(Resource.Id.txtWeekThursday);
            txtWeekFriday = rootView.FindViewById<TextView>(Resource.Id.txtWeekFriday);
            txtWeekSaturday = rootView.FindViewById<TextView>(Resource.Id.txtWeekSaturday);

            UpdateDateStrings();

            ivWeekBack.Click += async delegate
            {
                await ChangeWeek(-1);
            };
            ivWeekForward.Click += async delegate
            {
                await ChangeWeek(+1);
            };


            txtWeekSunday.Click += async delegate
            {
                await ChangeDay(0);
            };
            txtWeekMonday.Click += async delegate
            {
                await ChangeDay(1);
            };
            txtWeekTuesday.Click += async delegate
            {
                await ChangeDay(2);
            };
            txtWeekWednesday.Click += async delegate
            {
                await ChangeDay(3);
            };
            txtWeekThursday.Click += async delegate
            {
                await ChangeDay(4);
            };
            txtWeekFriday.Click += async delegate
            {
                await ChangeDay(5);
            };
            txtWeekSaturday.Click += async delegate
            {
                await ChangeDay(6);
            };
        }

        private async Task ChangeWeek(int weekDelta)
        {
            SelectedDate = SelectedDate.AddDays(7 * weekDelta);

            UpdateDateStrings();

            await SelectDate();
        }

        private async Task ChangeDay(int dayOfWeek)
        {
            var weekDelta = dayOfWeek -  (int) SelectedDate.DayOfWeek;
            SelectedDate = SelectedDate.AddDays(weekDelta);

            UpdateDateStrings();

            await SelectDate();
        }

        private void UpdateDateStrings()
        {
            txtCurrentWeek.Text = SelectedDate.ToString("MMMM yyyy");

            var dateSunday = SelectedDate.AddDays(-1 * (int)SelectedDate.DayOfWeek);
            txtWeekSunday.Text = $"S\n{dateSunday:dd}";
            txtWeekMonday.Text = $"M\n{dateSunday.AddDays(1):dd}";
            txtWeekTuesday.Text = $"T\n{dateSunday.AddDays(2):dd}";
            txtWeekWednesday.Text = $"W\n{dateSunday.AddDays(3):dd}";
            txtWeekThursday.Text = $"T\n{dateSunday.AddDays(4):dd}";
            txtWeekFriday.Text = $"F\n{dateSunday.AddDays(5):dd}";
            txtWeekSaturday.Text = $"S\n{dateSunday.AddDays(6):dd}";

            var white = Resources.GetColor(Resource.Color.opflow_white, null);
            var yellow = Resources.GetColor(Resource.Color.opflow_blue, null);
            txtWeekSunday.SetBackgroundColor(white);
            txtWeekMonday.SetBackgroundColor(white);
            txtWeekTuesday.SetBackgroundColor(white);
            txtWeekWednesday.SetBackgroundColor(white);
            txtWeekThursday.SetBackgroundColor(white);
            txtWeekFriday.SetBackgroundColor(white);
            txtWeekSaturday.SetBackgroundColor(white);
            
            TextView currentDate = null;
            switch (SelectedDate.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    currentDate = txtWeekSunday;
                    break;
                case DayOfWeek.Monday:
                    currentDate = txtWeekMonday;
                    break;
                case DayOfWeek.Tuesday:
                    currentDate = txtWeekTuesday;
                    break;
                case DayOfWeek.Wednesday:
                    currentDate = txtWeekWednesday;
                    break;
                case DayOfWeek.Thursday:
                    currentDate = txtWeekThursday;
                    break;
                case DayOfWeek.Friday:
                    currentDate = txtWeekFriday;
                    break;
                case DayOfWeek.Saturday:
                    currentDate = txtWeekSaturday;
                    break;
            }

            currentDate?.SetBackgroundColor(yellow);
        }

        protected abstract Task SelectDate();
    }
}