using Prism;
using Prism.Ioc;
using eDropletNFC.ViewModels;
using eDropletNFC.Views;
using Xamarin.Essentials.Interfaces;
using Xamarin.Essentials.Implementation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Resources;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;
using Xamarin.Essentials;
using System;
using System.Collections.Generic;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace eDropletNFC
{
    public partial class App
    {
        public static bool nfcScanFinshed;
        public static bool nfcScanTimeout;
        public static sensorData newSensorData = new sensorData();
        public static bool NfcError;
        public interface ILocale
        {
            string GetCurrent();
            void SetLocale();
        }
        public class L10n
        {
            public static void SetLocale()
            {
                DependencyService.Get<ILocale>().SetLocale();
            }
            public static string Locale()
            {
                return DependencyService.Get<ILocale>().GetCurrent();
            }
            public static string Localize(string key, string comment)
            {

                var netLanguage = Locale();
                // Platform-specific
                ResourceManager temp = new ResourceManager("Resx.AppResources", typeof(L10n).GetTypeInfo().Assembly);
                Debug.WriteLine("Localize " + key);
                string result = temp.GetString(key, new CultureInfo(netLanguage));

                return result;
            }
        }
        public static string CultureLocale { get; set; }
        public interface INFCscan
        {
            void NFCscan(byte mode);
        }
        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        protected override async void OnInitialized()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjY2NTY5QDMxMzgyZTMxMmUzMEhQY2QvaWd5UllVUGV5ZzArUmJqQWFoeGJZbE1BcE85Q2JtL1FlMVB3bEU9");
            //CultureLocale = DependencyService.Get<ILocale>().GetCurrent();
            
            nfcScanFinshed = false;
            nfcScanTimeout = false;
            NfcError = false;

            //Preferences.Set("preferencesInitiated", false);
            if (!Preferences.Get("preferencesInitiated", false))
            {
                Preferences.Set("preferencesInitiated", true);
                Preferences.Set("mmol", false);               
                Preferences.Set("targetLow", 80);
                Preferences.Set("targetHigh", 140);
                Preferences.Set("graphMax", 400);

                Debug.WriteLine("Preferences initialized");
            }
            else
            {
                Debug.WriteLine("@@@@ Preferences existing");
            }

            InitializeComponent();
           
            await NavigationService.NavigateAsync("NavigationPage/MasterPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<HomePage, HomePageViewModel>();
            containerRegistry.RegisterForNavigation<ToolsPage, ToolsPageViewModel>(); 
            containerRegistry.RegisterForNavigation<InfoPage, InfoPageViewModel>();
            containerRegistry.RegisterForNavigation<MasterPage, MasterPageViewModel>();
            containerRegistry.RegisterForNavigation<SetupPage, SetupPageViewModel>();
        }
        public class sensorData
        {
            public byte[] patchID = new byte[6];
            public byte[] patchUID = new byte[8];
            public byte[] Fram = new byte[344];
            public DateTime scanTime;
            public int libreType = -1;
            public string libreTypeTxt = "";
            public bool nfcScanResult = false;
            public string serialNo = "";
            public bool nfcScanReady = false;
            public float[] trend = new float[16];
            public float[] history = new float[32];
            public glucose currentGlucose;
            public List<glucose> historyGlucose;
            public string alarm;
            public bool isActionable;
            public bool lsaDetected;
            public int esaMinutesToWait;
            public DateTimeOffset startDate;
            public int battery;
            public int errCodeDecode;
            public string errStateDecode;
            public string errMsgDecode;
            public int infoProductFamily;
            public int infoError;
            public byte infoActivationCommand;
            public string infoActivationKey;
            public byte statusA1;
            public bool newSensorData;
            public bool isDataReading;
            public bool sensorActivated = false;
            public bool sensorInitialised = false;
        }
        public class glucose
        {
            public int id { get; set; }
            public int value { get; set; }
            public int status { get; set; }
        }

        public class BgValueRanges
        {
            public int inRangeLow;
            public int inRangeHigh;

            public int attRangeLow;
            public int attRangeHigh;

            public int alarmRangeLow;
            public int alarmRangeHigh;
        }
    }
}
