using Syncfusion.SfRangeSlider.XForms;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.Forms;
using eDropletNFC.ViewModels;

namespace eDropletNFC.Views
{
    public partial class SetupPage : ContentPage
    {
        public SetupPage()
        {
            InitializeComponent();
        }
        private void Mgdl_StateChanged(object sender, Syncfusion.XForms.Buttons.StateChangedEventArgs e)
        {
            if ((bool)e.IsChecked) Preferences.Set("mmol", false);
            else Preferences.Set("mmol", true);
            Debug.WriteLine("preference changed => " + Preferences.Get("mmol", false));
        }
        private void Mmol_StateChanged(object sender, Syncfusion.XForms.Buttons.StateChangedEventArgs e)
        {

        }
        private void TargetSlider_RangeChanging(object sender, Syncfusion.SfRangeSlider.XForms.RangeEventArgs e)
        {
            int rangeStart = (int)e.Start;
            int rangeEnd = (int)e.End;
            SfRangeSlider rangeSlider = e.RangeSlider;

            SetupPageViewModel.targetSelected(null, (int)rangeSlider.RangeStart, (int)rangeSlider.RangeEnd);
            Preferences.Set("targetLow", (int)rangeSlider.RangeStart);
            Preferences.Set("targetHigh", (int)rangeSlider.RangeEnd);

            var limitLow = Preferences.Get("targetLow", 80);
            var limitHigh = Preferences.Get("targetHigh", 140);
            Debug.WriteLine("Set Traget: " + limitLow.ToString() + "/" + limitHigh.ToString());
        }

        private void AlarmSlider_RangeChanging(object sender, Syncfusion.SfRangeSlider.XForms.RangeEventArgs e)
        {
            int rangeStart = (int)e.Start;
            int rangeEnd = (int)e.End;
            SfRangeSlider rangeSlider = e.RangeSlider;

            SetupPageViewModel.alarmSelected(null, (int)rangeSlider.RangeStart, (int)rangeSlider.RangeEnd);
            Preferences.Set("alarmLow", (int)rangeSlider.RangeStart);
            Preferences.Set("alarmHigh", (int)rangeSlider.RangeEnd);

            var limitLow = Preferences.Get("alarmLow", 20);
            var limitHigh = Preferences.Get("alarmHigh", 500);
            Debug.WriteLine("Set Alarm: " + limitLow.ToString() + "/" + limitHigh.ToString());

        }

    }
}
