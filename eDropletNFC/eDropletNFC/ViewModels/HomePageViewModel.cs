using eDropletNFC.Resx;
using eDropletNFC.Services;
using eDropletNFC.Views;
using Prism.Commands;
using Prism.Mvvm;
using Realms;
using Syncfusion.SfChart.XForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using static eDropletNFC.App;

namespace eDropletNFC.ViewModels
{
    public class HomePageViewModel : BindableBase
    {
        private string _homeTabTxt;
        public string homeTabTxt
        {
            get { return _homeTabTxt; }
            set { SetProperty(ref _homeTabTxt, value); }
        }
        private string _chartTitle;
        public string chartTitle
        {
            get { return _chartTitle; }
            set { SetProperty(ref _chartTitle, value); }
        }
        private string _homeT1Txt;
        public string homeT1Txt
        {
            get { return _homeT1Txt; }
            set { SetProperty(ref _homeT1Txt, value); }
        }
        private string _homeT2Txt;
        public string homeT2Txt
        {
            get { return _homeT2Txt; }
            set { SetProperty(ref _homeT2Txt, value); }
        }
        private string _homeT3Txt;
        public string homeT3Txt
        {
            get { return _homeT3Txt; }
            set { SetProperty(ref _homeT3Txt, value); }
        }
        private string _homeT4Txt;
        public string homeT4Txt
        {
            get { return _homeT4Txt; }
            set { SetProperty(ref _homeT4Txt, value); }
        }
        private string _homeBGTxt;
        public string homeBGTxt
        {
            get { return _homeBGTxt; }
            set { SetProperty(ref _homeBGTxt, value); }
        }
        private string _btnScanHomeTxt;
        public string btnScanHomeTxt
        {
            get { return _btnScanHomeTxt; }
            set { SetProperty(ref _btnScanHomeTxt, value); }
        }
        private int _bandStart;
        public int bandStart
        {
            get { return _bandStart; }
            set { SetProperty(ref _bandStart, value); }
        }
        private int _bandEnd;
        public int bandEnd
        {
            get { return _bandEnd; }
            set { SetProperty(ref _bandEnd, value); }
        }
        private int _graphMaxValue;
        public int graphMaxValue
        {
            get { return _graphMaxValue; }
            set { SetProperty(ref _graphMaxValue, value); }
        }
        private bool _busyIndHomeIsBusy;
        public bool busyIndHomeIsBusy
        {
            get { return _busyIndHomeIsBusy; }
            set { SetProperty(ref _busyIndHomeIsBusy, value); }
        }

        public DelegateCommand btnScanHomeCmd { get; set; }

        private ObservableCollection<ChartDataPoint> _graphData;
        public ObservableCollection<ChartDataPoint> graphData 
        {
            get { return _graphData; }
            set { SetProperty(ref _graphData, value); }
        }
        public ObservableCollection<ChartDataPoint> pointDataT { get; set; }
        public ObservableCollection<ChartDataPoint> pointDataH { get; set; }
        private ObservableCollection<ChartDataPoint> _nullData;
        public ObservableCollection<ChartDataPoint> nullData
        {
            get { return _nullData; }
            set { SetProperty(ref _nullData, value); }
        }

        public static double mmValue = 18018018 / 1000000;
        public static INFCscan _scan;

        public HomePageViewModel(INFCscan scan)
        {
            
            _scan = scan;
            homeTabTxt = AppResources.toolbarHome;
            chartTitle = Preferences.Get("mmol", false) ? AppResources.chartTitleMmol : AppResources.chartTitle; 
            homeT1Txt = AppResources.homeT1Txt;
            homeT2Txt = AppResources.homeT2Txt;
            homeT3Txt = AppResources.homeT3Txt;
            homeT4Txt = AppResources.homeT4Txt;
            homeBGTxt = AppResources.homeBGTxt;
            btnScanHomeTxt = AppResources.btnScanHomeTxt;
            graphData = new ObservableCollection<ChartDataPoint>();
            pointDataT = new ObservableCollection<ChartDataPoint>();
            pointDataH = new ObservableCollection<ChartDataPoint>();
            nullData = new ObservableCollection<ChartDataPoint>();
            busyIndHomeIsBusy = false;

            
            if (Preferences.Get("mmol", false))
            {                
                bandStart = (int)(Preferences.Get("targetLow", 80) / mmValue);
                bandEnd = (int)(Preferences.Get("targetHigh", 140) - Preferences.Get("targetLow", 80) / mmValue);
            }
            else
            {               
                bandStart = (int)Preferences.Get("targetLow", 80);
                bandEnd = (int)(Preferences.Get("targetHigh", 140) - Preferences.Get("targetLow", 80));
            }
            graphMaxValue = (int)(Preferences.Get("graphMax", 300));

            

            string message2 = "targetChanged";
            MessagingCenter.Subscribe<SetupPageViewModel.MyClass2>(this, message2, (arg) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Debug.WriteLine("&&&&&&& msg received in Home Screen from changing target");
                    bandStart = (int)Preferences.Get("targetLow", 80);
                    bandEnd = (int)(Preferences.Get("targetHigh", 140) - (int)Preferences.Get("targetLow", 80));
                    Debug.WriteLine("&&&&&&& new target: " + bandStart.ToString() + " - " + (bandStart + bandEnd).ToString());
                });

            });
            btnScanHomeCmd = new DelegateCommand(btnScanCmdAsync);

            string message3 = "nfcScanDone";
            MessagingCenter.Subscribe<HomePageViewModel.MyClass4>(this, message3, (arg) =>
            {
                graphData.Clear();
                pointDataH.Clear();
                pointDataT.Clear();
                nullData.Clear();
                System.DateTimeOffset endTimeMarker = DateTime.Now.ToUniversalTime();
                System.DateTimeOffset endTimeMarker1;
                endTimeMarker1 = endTimeMarker.AddHours(2);
                nullData.Add(new ChartDataPoint(endTimeMarker1.ToLocalTime().DateTime, 0.0));

                System.DateTimeOffset timestamp = DateTime.Now.ToUniversalTime();
                System.DateTimeOffset timestamp1;


                homeT4Txt = "NFC scan OK";
                homeT4Txt += "\r\nTime: " + DateTime.Now.ToShortTimeString();
                homeT1Txt = "Type: " + App.newSensorData.libreTypeTxt;
                homeT1Txt += "\r\nS/N: " + App.newSensorData.serialNo;
                homeT1Txt += "\r\nStatus: " + Common.decodeStat(App.newSensorData.Fram[4]);
                int[] trend = Common.calcTrend(App.newSensorData.Fram, App.newSensorData.Fram[26]);
                string trendTxt = "";
                
                for (int i = 0; i < 16; i++)
                {
                    trendTxt += (trend[i] / 10.0).ToString() + " ";
                    timestamp1 = timestamp.AddMinutes(-(i * 1));
                    App.newSensorData.trend[i] = (float)(trend[i] / 10.0);
                    pointDataT.Add(new ChartDataPoint(timestamp1.ToLocalTime().DateTime, Preferences.Get("mmol", false) ? App.newSensorData.trend[i] / mmValue : App.newSensorData.trend[i]));
                }
                homeBGTxt = ((int)trend[0]).ToString();
                Debug.WriteLine("T: -> " + trendTxt);

                int[] history = Common.calcHistory(App.newSensorData.Fram, App.newSensorData.Fram[27]);
                string historyTxt = "";
                timestamp = DateTime.Now.ToUniversalTime();
                for (int i = 0; i < 32; i++)
                {
                    timestamp1 = timestamp.AddMinutes(-(i * 15));
                    historyTxt += (history[i] / 10.0).ToString() + " ";
                    App.newSensorData.history[i] = (float)(history[i] / 10.0);
                    pointDataH.Add(new ChartDataPoint(timestamp1.ToLocalTime().DateTime, Preferences.Get("mmol", false) ? App.newSensorData.history[i] / mmValue : App.newSensorData.history[i]));
                    graphData.Add(new ChartDataPoint(timestamp1.ToLocalTime().DateTime, Preferences.Get("mmol", false) ? App.newSensorData.history[i] / mmValue : App.newSensorData.history[i]));                    
                }
                Debug.WriteLine("H: -> " + historyTxt);
                Debug.WriteLine("count: " + pointDataH.Count.ToString());
            });
        }

        public async void btnScanCmdAsync()
        {
            
            Debug.WriteLine("NFC scan button clicked");
            busyIndHomeIsBusy = true;

            App.nfcScanFinshed = false;
            App.nfcScanTimeout = false;

            _scan.NFCscan(1);
            while (App.nfcScanFinshed == false) await Task.Delay(10);
            busyIndHomeIsBusy = false;

            if (App.NfcError)
            {
                homeT4Txt = "NFC error";
                Debug.WriteLine("after nfc scan - error");
                await Task.Delay(5000);
                homeT4Txt = "...";
                return;
            }
            if (App.nfcScanTimeout == true)
            {
                homeT4Txt = "NFC timeout";
                Debug.WriteLine("after nfc scan - timeout");
                await Task.Delay(5000);
                homeT4Txt = "...";
                return;
            }           
            MyClass4.CaptureDelegate();
            MyClass4.RunNonStaticMethod();
            return;
        }
        public class MyClass4
        {
            private static Action NonStaticDelegate;
            public void NonStaticMethod()
            {
                MessagingCenter.Send<MyClass4>(this, "nfcScanDone");
            }
            public static void CaptureDelegate()
            {
                MyClass4 temp = new MyClass4();
                MyClass4.NonStaticDelegate = new Action(temp.NonStaticMethod);
            }
            public static void RunNonStaticMethod()
            {
                if (MyClass4.NonStaticDelegate != null)
                {
                    MyClass4.NonStaticDelegate();
                }
            }
        }
    }
}
