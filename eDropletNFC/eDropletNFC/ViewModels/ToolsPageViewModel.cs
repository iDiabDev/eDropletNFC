using eDropletNFC.Resx;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static eDropletNFC.App;

namespace eDropletNFC.ViewModels
{
    public class ToolsPageViewModel : BindableBase
    {
        public string toolsTabTxt { get; private set; }
        public string btnToolsInitTxt { get; private set; }
        public string btnToolsActivTxt { get; private set; }

        private string _toolsLblTxt;
        public string toolsLblTxt
        {
            get { return _toolsLblTxt; }
            set { SetProperty(ref _toolsLblTxt, value); }
        }
        public DelegateCommand btnToolsInitCmd { get; set; }
        public DelegateCommand btnToolsActivCmd { get; set; }

        private bool _busyIndToolsIsBusy;
        public bool busyIndToolsIsBusy
        {
            get { return _busyIndToolsIsBusy; }
            set { SetProperty(ref _busyIndToolsIsBusy, value); }
        }
        public static INFCscan _scan;
        public ToolsPageViewModel(INFCscan scan)
        {
            _scan = scan;
            toolsTabTxt = AppResources.toolbarTools;
            btnToolsInitTxt = "Re-Initialze Sensor";
            btnToolsActivTxt = "Activate Sensor";
            toolsLblTxt = "...";
            btnToolsInitCmd = new DelegateCommand(btnToolsInitCmdAsync);
            btnToolsActivCmd = new DelegateCommand(btnToolsActivCmdAsync);
            busyIndToolsIsBusy = false;
        }        
        private async void btnToolsInitCmdAsync()
        {
            Debug.WriteLine("&&&&&&& init cmd");
            busyIndToolsIsBusy = true;

            App.nfcScanFinshed = false;
            App.nfcScanTimeout = false;
            toolsLblTxt = "...";
            _scan.NFCscan(2);
            while (App.nfcScanFinshed == false) await Task.Delay(10);
            busyIndToolsIsBusy = false;

            if (App.NfcError)
            {
                toolsLblTxt = "NFC error" + "\r\n";
                toolsLblTxt += "--end.";
                Debug.WriteLine("after nfc scan - error");
                await Task.Delay(5000);
                toolsLblTxt = "...";
                return;
            }
            if (App.nfcScanTimeout == true)
            {
                toolsLblTxt = "NFC timeout" + "\r\n";
                toolsLblTxt += "--end.";
                Debug.WriteLine("after nfc scan - timeout");
                await Task.Delay(5000);
                toolsLblTxt = "...";
                return;
            }
            toolsLblTxt = "NFC scan OK \r\n";
            toolsLblTxt += "Sensor: " + App.newSensorData.libreTypeTxt + "\r\n";
            toolsLblTxt += "S/N: " + App.newSensorData.serialNo + "\r\n";
            var statusByte = App.newSensorData.Fram[4];
            if ((statusByte == 0x03) || (statusByte == 0x05))    // active or expired
            {
                toolsLblTxt += "Status: " + App.newSensorData.Fram[4].ToString("X2") + " - OK -> Re-Initialising Sensor" + "\r\n";
            }
            else if ((statusByte == 0x01))
            {
                toolsLblTxt += "Status: " + App.newSensorData.Fram[4].ToString("X2") + " - Sensor ready (new) -> Activate" + "\r\n";
                toolsLblTxt += "--end.";
                return;
            }
            else
            {
                toolsLblTxt += "Status: " + App.newSensorData.Fram[4].ToString("X2") + " - Re-init not possible" + "\r\n";
                toolsLblTxt += "--end.";
                return;
            }    

            if (App.newSensorData.sensorInitialised)
            {
                toolsLblTxt += "Sensor re-initialized -> Activate.\r\n";
                toolsLblTxt += "--end.";
            }
            else
            {
                toolsLblTxt += "Error during re-initialisation - retry.\r\n";
                toolsLblTxt += "--end.";
            }
        }

        private async void btnToolsActivCmdAsync()
        {
            Debug.WriteLine("&&&&&&& activ cmd");
            busyIndToolsIsBusy = true;

            App.nfcScanFinshed = false;
            App.nfcScanTimeout = false;
            toolsLblTxt = "...";
            _scan.NFCscan(3);
            while (App.nfcScanFinshed == false) await Task.Delay(10);
            busyIndToolsIsBusy = false;

            if (App.NfcError)
            {
                toolsLblTxt = "NFC error" + "\r\n";
                toolsLblTxt += "--end.";
                Debug.WriteLine("after nfc scan - error");
                await Task.Delay(5000);
                toolsLblTxt = "...";
                return;
            }
            if (App.nfcScanTimeout == true)
            {
                toolsLblTxt = "NFC timeout" + "\r\n";
                toolsLblTxt += "--end.";
                Debug.WriteLine("after nfc scan - timeout");
                await Task.Delay(5000);
                toolsLblTxt = "...";
                return;
            }
            toolsLblTxt = "NFC scan OK \r\n";
            toolsLblTxt += "Sensor: " + App.newSensorData.libreTypeTxt + "\r\n";
            toolsLblTxt += "S/N: " + App.newSensorData.serialNo + "\r\n";
            if (App.newSensorData.sensorActivated)
            {
                toolsLblTxt += "Sensoractivated -> wait 60 minutes for warm-up.\r\n";
                toolsLblTxt += "--end.";
            }
            else
            {
                toolsLblTxt += "Error during activation - retry.\r\n";
                toolsLblTxt += "--end.";
            }
        }

    }
}
