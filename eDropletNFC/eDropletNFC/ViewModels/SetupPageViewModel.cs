using eDropletNFC.Resx;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace eDropletNFC.ViewModels
{
    public class SetupPageViewModel : BindableBase
    {
        public string setupTabTxt { get; private set; }
        public bool isMgdlSelected { get; private set; }
        public bool isMmolSelected { get; private set; }
        public float targetSetupStart { get; private set; }
        public float targetSetupEnd { get; private set; }
        public float alarmSetupLow { get; private set; }
        public float alarmSetupHigh { get; private set; }
        public string BGtarget { get; private set; }
        public string BGunits { get; private set; }
        public SetupPageViewModel()
        {
            setupTabTxt = AppResources.toolbarSetup;
            targetSetupStart = Preferences.Get("targetLow", 80);
            targetSetupEnd = Preferences.Get("targetHigh", 140);
            alarmSetupLow = Preferences.Get("alarmLow", 20);
            alarmSetupHigh = Preferences.Get("alarmHigh", 500);
            isMgdlSelected = !Preferences.Get("mmol", false);
            isMmolSelected = Preferences.Get("mmol", false);
            BGtarget = AppResources.BGtarget;
            BGunits = AppResources.BGunits;

        }
        public async static void targetSelected(object sender, int value1, int value2)
        {

            Debug.WriteLine("target changed");
            MyClass2.CaptureDelegate();
            MyClass2.RunNonStaticMethod();
        }
        public async static void alarmSelected(object sender, int value1, int value2)
        {
            await Task.Delay(500);
            Debug.WriteLine("alarm changed");
            MyClass3.CaptureDelegate();
            MyClass3.RunNonStaticMethod();
        }
        public class MyClass2
        {
            private static Action NonStaticDelegate;
            public void NonStaticMethod()
            {

                MessagingCenter.Send<MyClass2>(this, "targetChanged");
            }
            public static void CaptureDelegate()
            {
                MyClass2 temp = new MyClass2();
                MyClass2.NonStaticDelegate = new Action(temp.NonStaticMethod);
            }
            public static void RunNonStaticMethod()
            {
                if (MyClass2.NonStaticDelegate != null)
                {
                    MyClass2.NonStaticDelegate();
                }
            }
        }
        public class MyClass3
        {
            private static Action NonStaticDelegate;
            public void NonStaticMethod()
            {
                MessagingCenter.Send<MyClass3>(this, "alarmChanged");
            }
            public static void CaptureDelegate()
            {
                MyClass3 temp = new MyClass3();
                MyClass3.NonStaticDelegate = new Action(temp.NonStaticMethod);
            }
            public static void RunNonStaticMethod()
            {
                if (MyClass3.NonStaticDelegate != null)
                {
                    MyClass3.NonStaticDelegate();
                }
            }
        }
    }
    
}
