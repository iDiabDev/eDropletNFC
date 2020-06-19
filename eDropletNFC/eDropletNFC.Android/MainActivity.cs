using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;
using eDropletNFC.Services;
using Poz1.NFCForms.Droid;
using Prism;
using Prism.Ioc;
using System;
using System.Threading.Tasks;
using static eDropletNFC.App;
using Debug = System.Diagnostics.Debug;

namespace eDropletNFC.Droid
{
    [Activity(Label = "eDropletNFC", 
        Icon = "@mipmap/ic_launcher", 
        Theme = "@style/MainTheme",
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [MetaData(NfcAdapter.ActionTechDiscovered, Resource = "@xml/nfc_tech_filter")]
    [IntentFilter(new[] { NfcAdapter.ActionTechDiscovered })]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public NfcAdapter NFCdevice;
        public Context context;
        public static Vibrator vibrator;
        public static Intent intent1;
        protected override void OnCreate(Bundle savedInstanceState)
        {

            vibrator = (Vibrator)this.ApplicationContext.GetSystemService(Context.VibratorService);

            base.OnCreate(savedInstanceState);
            
            NfcManager NfcManager = (NfcManager)Android.App.Application.Context.GetSystemService(Context.NfcService);
            NFCdevice = NfcManager.DefaultAdapter;

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App(new AndroidInitializer()));
            this.RequestPermissions(new[]
           {
                Manifest.Permission.Nfc,
                Manifest.Permission.Vibrate,
                Manifest.Permission.WriteExternalStorage,
                Manifest.Permission.ReceiveBootCompleted
            }, 0);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        protected override void OnNewIntent(Intent intent)
        {
            intent1 = intent;
            base.OnNewIntent(intent);
        }
        protected override void OnResume()
        {
            Debug.WriteLine(">>>>>>>>>>>>> entered OnResume");
            base.OnResume();

            if (NFCdevice != null)
            {
                var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
                NFCdevice.EnableForegroundDispatch
                (
                    this,
                    PendingIntent.GetActivity(this, 0, intent, 0),
                    new[] { new IntentFilter(NfcAdapter.ActionTechDiscovered) },
                    new System.String[][] {new string[] {
                            NFCTechs.Ndef,
                        },
                        new string[] {
                            NFCTechs.NfcV,
                        },
                    }
                );
            }
        }
    }
    public class nfcScan : INFCscan
    {
        public static string cmdErrorDesc = "";
        public async void NFCscan(byte mode)
        {
           // byte mode = 1;
            Debug.WriteLine("entry into Android NFC with mode " + mode.ToString());
            bool ok = false; ;
            NfcV iso15693 = null;
            var timeStart = DateTime.Now.Ticks;
            Tag tag = null;

            byte[] uid = new byte[8];
            try
            {
                MainActivity.vibrator.Vibrate(500);
            }
            catch (Exception exA)
            {
                Debug.WriteLine("error Android: " + exA.Message);
            }
            while (true)
            {
                if ((DateTime.Now.Ticks - timeStart) > (20 * 10000000))
                {
                    App.nfcScanFinshed = true;
                    App.nfcScanTimeout = true;
                    return;
                }
                try
                {
                    tag = (Tag)MainActivity.intent1.GetParcelableExtra(NfcAdapter.ExtraTag);
                    iso15693 = NfcV.Get(tag);
                    iso15693.Connect();
                    ok = true;
                    Debug.WriteLine("NFC connect ");
                }
                catch (Exception exB)
                {
                    Debug.WriteLine("error Android: " + exB.Message);
                    ok = false;
                }
                if (ok) break;
                await Task.Delay(100);
            }
            try
            {
                byte[] result1 = nfcRW(iso15693, new byte[] { 0x02, 0xA1, 0x07 });
                if (checkResultNFC(result1))
                {
                    App.newSensorData.nfcScanReady = true;
                    App.newSensorData.nfcScanResult = false;
                    Debug.WriteLine("return from tag lost in  A1 07");
                    iso15693.Close();
                    tag.Dispose();
                    MainActivity.vibrator.Vibrate(100);
                    return;
                }
                Array.Copy(result1, 1, App.newSensorData.patchID, 0, 6);
                Debug.WriteLine("A1 07 : " + Common.ByteArrayToString(result1));
            }
            catch (System.Exception)
            {
                App.newSensorData.nfcScanResult = false;
                return;
            }
            try
            {
                byte[] result2 = nfcRW(iso15693, new byte[] { 0x26, 0x01, 0x00 });
                if (checkResultNFC(result2))
                {
                    App.newSensorData.nfcScanReady = true;
                    App.newSensorData.nfcScanResult = false;
                    Debug.WriteLine("return from tag lost in  26 01");
                    iso15693.Close();
                    tag.Dispose();
                    MainActivity.vibrator.Vibrate(100);
                    return;
                };
                Array.Copy(result2, 2, App.newSensorData.patchUID, 0, result2.Length - 2);
                App.newSensorData.serialNo = Common.getSensorSerialNumber(Common.checkSensorType(App.newSensorData.patchID), App.newSensorData.patchUID);
                Debug.WriteLine("26 01: " + Common.ByteArrayToString(result2));
            }
            catch (System.Exception)
            {
                App.newSensorData.nfcScanResult = false;
                return;
            }
            Debug.WriteLine("Sensor data: " + App.newSensorData.libreTypeTxt.ToString() + " - " + App.newSensorData.serialNo);

            switch (mode)
            {
                case 1:                                     // only FRAM read
                    switch (App.newSensorData.libreType)
                    {
                        case 1:         // Libre 1
                            try
                            {

                                for (byte b = 0; b < 0x2A; b++)
                                {
                                    byte[] result4 = nfcRW(iso15693, new byte[] { 0x02, 0x23, b, 1 });
                                    App.NfcError = false;
                                    if (checkResultNFC(result4))
                                    {
                                        App.newSensorData.nfcScanReady = true;
                                        App.newSensorData.nfcScanResult = false;
                                        Debug.WriteLine("return from tag lost in  first loop");
                                        iso15693.Close();
                                        tag.Dispose();
                                        MainActivity.vibrator.Vibrate(100);
                                        return;
                                    };
                                    Array.Copy(result4, 1, App.newSensorData.Fram, b * 8, 16);
                                    Debug.WriteLine("S: 0x" + b.ToString("X2") + " -> " + Common.ByteArrayToString(result4));
                                    byte[] res = new byte[16];
                                    Array.Copy(result4, 1, res, 0, 16);
                                    b++;
                                }
                                byte[] result4a = nfcRW(iso15693, new byte[] { 0x02, 0x23, 0x2A, 0 });
                                Debug.WriteLine("S: 0x2A -> " + Common.ByteArrayToString(result4a));
                                App.NfcError = false;
                                if (checkResultNFC(result4a))
                                {
                                    App.newSensorData.nfcScanReady = true;
                                    App.newSensorData.nfcScanResult = false;
                                    Debug.WriteLine("return from tag lost in  add to loop");
                                    iso15693.Close();
                                    tag.Dispose();
                                    MainActivity.vibrator.Vibrate(100);
                                    return;
                                };
                                byte[] resa = new byte[8];
                                Array.Copy(result4a, 1, resa, 0, 8);
                                Array.Copy(result4a, 1, App.newSensorData.Fram, (344 - 8), 8);
                                Debug.WriteLine("F: -> " + Common.ByteArrayToString(App.newSensorData.Fram));


                            }
                            catch (Exception)
                            {
                                App.newSensorData.nfcScanReady = true;
                                App.newSensorData.nfcScanResult = false;
                                iso15693.Close();
                                tag.Dispose();
                                MainActivity.vibrator.Vibrate(100);
                                return;
                            }

                            break;
                        default:        // all other sensor types
                            break;
                    }
                    break;
                case 2:                                     // FRAM read + re-initialize
                    switch (App.newSensorData.libreType)
                    {
                        case 1:         // Libre 1
                            try
                            {
                                App.newSensorData.sensorActivated = false;
                                for (byte b = 0; b < 0x2A; b++)
                                {
                                    byte[] result4 = nfcRW(iso15693, new byte[] { 0x02, 0x23, b, 1 });
                                    App.NfcError = false;
                                    if (checkResultNFC(result4))
                                    {
                                        App.newSensorData.nfcScanReady = true;
                                        App.nfcScanFinshed = true;
                                        App.newSensorData.nfcScanResult = false;
                                        App.newSensorData.nfcScanResult = false;
                                        Debug.WriteLine("return from tag lost in  first loop");
                                        iso15693.Close();
                                        tag.Dispose();
                                        MainActivity.vibrator.Vibrate(100);
                                        return;
                                    };
                                    Array.Copy(result4, 1, App.newSensorData.Fram, b * 8, 16);
                                    Debug.WriteLine("S: 0x" + b.ToString("X2") + " -> " + Common.ByteArrayToString(result4));
                                    byte[] res = new byte[16];
                                    Array.Copy(result4, 1, res, 0, 16);
                                    b++;
                                }
                                byte[] result4a = nfcRW(iso15693, new byte[] { 0x02, 0x23, 0x2A, 0 });
                                Debug.WriteLine("S: 0x2A -> " + Common.ByteArrayToString(result4a));
                                App.NfcError = false;
                                if (checkResultNFC(result4a))
                                {
                                    App.newSensorData.nfcScanReady = true;
                                    App.newSensorData.nfcScanResult = false;
                                    App.nfcScanFinshed = true;
                                    Debug.WriteLine("return from tag lost in  add to loop");
                                    iso15693.Close();
                                    tag.Dispose();
                                    MainActivity.vibrator.Vibrate(100);
                                    return;
                                };
                                byte[] resa = new byte[8];
                                Array.Copy(result4a, 1, resa, 0, 8);
                                Array.Copy(result4a, 1, App.newSensorData.Fram, (344 - 8), 8);
                                Debug.WriteLine("F: -> " + Common.ByteArrayToString(App.newSensorData.Fram));
                            }
                            catch (Exception)
                            {
                                App.newSensorData.sensorInitialised = false;
                                App.newSensorData.nfcScanReady = true;
                                App.newSensorData.nfcScanResult = false;
                                App.nfcScanFinshed = true;
                                iso15693.Close();
                                tag.Dispose();
                                MainActivity.vibrator.Vibrate(100);
                                return;
                            }
                            // initialise
                            var statusByte = App.newSensorData.Fram[4];
                            if ((statusByte == 0x03) || (statusByte == 0x05))    // active or expired
                            {
                                byte[] dataCRCh = new byte[22];
                                byte[] dataCRCb = new byte[294];
                                byte[] dataCRCf = new byte[22];

                                Array.Copy(App.newSensorData.Fram, 2, dataCRCh, 0, 22);
                                ushort crcNewH = Common.computeCRC16x(dataCRCh, 22, 0, 0);
                                Debug.WriteLine("CRC-H: -> " + crcNewH.ToString("X4"));

                                Array.Copy(App.newSensorData.Fram, 26, dataCRCb, 0, 294);
                                ushort crcNewB = Common.computeCRC16x(dataCRCb, 294, 0, 0);
                                Debug.WriteLine("CRC-B: -> " + crcNewB.ToString("X4"));

                                Array.Copy(App.newSensorData.Fram, 322, dataCRCf, 0, 22);
                                ushort crcNewF = Common.computeCRC16x(dataCRCf, 22, 0, 0);
                                Debug.WriteLine("CRC-F: -> " + crcNewF.ToString("X4"));

                                Debug.WriteLine(Common.ByteArrayToString(App.newSensorData.Fram));
                                for (int i = 26; i < 319; i++) App.newSensorData.Fram[i] = 0x00;

                                App.newSensorData.Fram[4] = 0x01;
                                for (int m = 5; m < 24; m++) App.newSensorData.Fram[m] = 0x00;
                                Array.Copy(App.newSensorData.Fram, 2, dataCRCh, 0, 22);
                                crcNewH = Common.computeCRC16x(dataCRCh, 22, 0, 0);

                                byte b1 = (byte)(crcNewH >> 8);
                                byte b0 = (byte)(crcNewH & 255);
                                Debug.WriteLine("b0: -> " + b0.ToString("X2"));
                                Debug.WriteLine("b1: -> " + b1.ToString("X2"));
                                App.newSensorData.Fram[0] = b0;
                                App.newSensorData.Fram[1] = b1;
                                App.newSensorData.Fram[24] = 0x62;
                                App.newSensorData.Fram[25] = 0xC2;

                                Debug.WriteLine(Common.ByteArrayToString(App.newSensorData.Fram));
                                Array.Copy(App.newSensorData.Fram, 2, dataCRCh, 0, 22);
                                crcNewH = Common.computeCRC16x(dataCRCh, 22, 0, 0);
                                Debug.WriteLine("CRC-H: -> " + crcNewH.ToString("X4"));

                                Array.Copy(App.newSensorData.Fram, 26, dataCRCb, 0, 294);
                                crcNewB = Common.computeCRC16x(dataCRCb, 294, 0, 0);
                                Debug.WriteLine("CRC-B: -> " + crcNewB.ToString("X4"));

                                Array.Copy(App.newSensorData.Fram, 322, dataCRCf, 0, 22);
                                crcNewF = Common.computeCRC16x(dataCRCf, 22, 0, 0);
                                Debug.WriteLine("CRC-F: -> " + crcNewF.ToString("X4"));

                                unlockChip(iso15693);
                                byte[] cmd;
                                for (byte k = 0; k < 0x2B; k++)
                                {
                                    byte w0 = App.newSensorData.Fram[(k * 8) + 0];
                                    byte w1 = App.newSensorData.Fram[(k * 8) + 1];
                                    byte w2 = App.newSensorData.Fram[(k * 8) + 2];
                                    byte w3 = App.newSensorData.Fram[(k * 8) + 3];
                                    byte w4 = App.newSensorData.Fram[(k * 8) + 4];
                                    byte w5 = App.newSensorData.Fram[(k * 8) + 5];
                                    byte w6 = App.newSensorData.Fram[(k * 8) + 6];
                                    byte w7 = App.newSensorData.Fram[(k * 8) + 7];

                                    cmd = new byte[] { 0x02, 0x21, k, w0, w1, w2, w3, w4, w5, w6, w7 };
                                    byte[] result2u = nfcRW(iso15693, cmd);
                                }
                                lockChip(iso15693);
                                App.newSensorData.sensorInitialised = true;
                                App.nfcScanFinshed = true;
                                Debug.WriteLine("sensor locked - end procedure ");
                            }
                            else
                            {
                                Debug.WriteLine("not initialized - end procedure ");
                                App.newSensorData.sensorInitialised = false;
                                App.newSensorData.nfcScanReady = true;
                                App.newSensorData.nfcScanResult = true;
                                App.nfcScanFinshed = true;
                                iso15693.Close();
                                tag.Dispose();
                                return;
                            }

                            break;
                        default:        // all other sensor types
                            App.newSensorData.sensorInitialised = false;
                            App.newSensorData.nfcScanReady = true;
                            App.newSensorData.nfcScanResult = false;
                            App.nfcScanFinshed = true;
                            iso15693.Close();
                            tag.Dispose();
                            break;
                    }
                    iso15693.Close();
                    tag.Dispose();
                    App.newSensorData.nfcScanReady = true;
                    App.newSensorData.nfcScanResult = true;
                    App.nfcScanFinshed = true;
                    break;
                case 3:                                     // FRAM read + activate
                    switch (App.newSensorData.libreType)
                    {
                        case 1:         // Libre 1
                            try
                            {
                                App.newSensorData.sensorInitialised = false;
                                for (byte b = 0; b < 0x2A; b++)
                                {
                                    byte[] result4 = nfcRW(iso15693, new byte[] { 0x02, 0x23, b, 1 });
                                    App.NfcError = false;
                                    if (checkResultNFC(result4))
                                    {
                                        App.newSensorData.nfcScanReady = true;
                                        App.nfcScanFinshed = true;
                                        App.newSensorData.nfcScanResult = false;
                                        App.newSensorData.nfcScanResult = false;
                                        Debug.WriteLine("return from tag lost in  first loop");
                                        iso15693.Close();
                                        tag.Dispose();
                                        MainActivity.vibrator.Vibrate(100);
                                        return;
                                    };
                                    Array.Copy(result4, 1, App.newSensorData.Fram, b * 8, 16);
                                    Debug.WriteLine("S: 0x" + b.ToString("X2") + " -> " + Common.ByteArrayToString(result4));
                                    byte[] res = new byte[16];
                                    Array.Copy(result4, 1, res, 0, 16);
                                    b++;
                                }
                                byte[] result4a = nfcRW(iso15693, new byte[] { 0x02, 0x23, 0x2A, 0 });
                                Debug.WriteLine("S: 0x2A -> " + Common.ByteArrayToString(result4a));
                                App.NfcError = false;
                                if (checkResultNFC(result4a))
                                {
                                    App.newSensorData.nfcScanReady = true;
                                    App.newSensorData.nfcScanResult = false;
                                    App.nfcScanFinshed = true;
                                    Debug.WriteLine("return from tag lost in  add to loop");
                                    iso15693.Close();
                                    tag.Dispose();
                                    MainActivity.vibrator.Vibrate(100);
                                    return;
                                };
                                byte[] resa = new byte[8];
                                Array.Copy(result4a, 1, resa, 0, 8);
                                Array.Copy(result4a, 1, App.newSensorData.Fram, (344 - 8), 8);
                                Debug.WriteLine("F: -> " + Common.ByteArrayToString(App.newSensorData.Fram));
                            }
                            catch (Exception)
                            {
                                App.newSensorData.sensorActivated = false;
                                App.newSensorData.nfcScanReady = true;
                                App.newSensorData.nfcScanResult = false;
                                App.nfcScanFinshed = true;
                                iso15693.Close();
                                tag.Dispose();
                                MainActivity.vibrator.Vibrate(100);
                                return;
                            }
                            // activate
                            var statusByte = App.newSensorData.Fram[4];
                            if (statusByte == 0x01)    // ready to activate
                            {
                                byte[] cmd = new byte[] { 0x02, 0xA0, 0x07, 0xC2, 0xAD, 0x75, 0x21 };
                                byte[] result2u = nfcRW(iso15693, cmd);
                                Debug.WriteLine("Result activation: " + Common.ByteArrayToString(result2u));
                                App.nfcScanFinshed = true;
                                App.newSensorData.sensorActivated = true;
                            }
                            else
                            {
                                Debug.WriteLine("not activated - end procedure ");
                                App.newSensorData.sensorActivated = false;
                                App.newSensorData.nfcScanReady = true;
                                App.newSensorData.nfcScanResult = true;
                                App.nfcScanFinshed = true;
                                iso15693.Close();
                                tag.Dispose();
                                return;
                            }

                            break;
                        default:        // all other sensor types
                            App.newSensorData.sensorActivated = false;
                            App.newSensorData.nfcScanReady = true;
                            App.newSensorData.nfcScanResult = false;
                            App.nfcScanFinshed = true;
                            iso15693.Close();
                            tag.Dispose();
                            break;
                    }
                    iso15693.Close();
                    tag.Dispose();
                    App.newSensorData.nfcScanReady = true;
                    App.newSensorData.nfcScanResult = true;
                    App.nfcScanFinshed = true;
                    break;

                default:                                    // all other modes
                    break;

            }
            iso15693.Close();
            tag.Dispose();
            MainActivity.vibrator.Vibrate(100);
            App.nfcScanTimeout = false;
            App.nfcScanFinshed = true;
            return;
        }
        private void unlockChip(NfcV iso15693)
        {
            byte[] cmd = new byte[] { 0x00 };
            if (App.newSensorData.libreType == 1)
            {
                cmd = new byte[] { 0x02, 0xA4, 0x07, 0xC2, 0xAD, 0x75, 0x21 }; // OK
            }
            else if (App.newSensorData.libreType == 2)
            {
                cmd = new byte[] { 0x02, 0xA4, 0x07, 0x1B, 0x60, 0xB2, 0x4B, 0x2A }; // 1B 60 B2 4B 2A
            }
            if (App.newSensorData.libreType == 3)
            {
                cmd = new byte[] { 0x02, 0xA4, 0x07, 0x1B, 0x75, 0xAE, 0x93, 0xF0 }; // 1B 75 AE 93 F0
            }
            if (App.newSensorData.libreType == 4)
            {
                cmd = new byte[] { 0x02, 0xA4, 0x07, 0xC2, 0xAD, 0x00, 0x90 }; // OK
            }
            byte[] result2u = nfcRW(iso15693, cmd);
            Debug.WriteLine("Result unlock: " + Common.ByteArrayToString(result2u));
        }
        private void lockChip(NfcV iso15693)
        {
            byte[] cmd = new byte[] { 0x02, 0xA2, 0x07, 0xC2, 0xAD, 0x75, 0x21 };
            byte[] result2u = nfcRW(iso15693, cmd);
            Debug.WriteLine("Result lock: " + Common.ByteArrayToString(result2u));
        }
        public static bool checkResultNFC(byte[] result)
        {
            if (result[0] == 0xFF && result[2] < 0x0F && result[1] == 0x00) return true;
            else return false;
        }

        public static byte[] nfcRW(NfcV nfcHandle, byte[] cmd)
        {
            cmdErrorDesc = DateTime.Now.ToShortTimeString() + " - " + Common.ByteArrayToString(cmd) + " - ";
            try
            {
                byte[] result = nfcHandle.Transceive(cmd);
                cmdErrorDesc += "OK";

                return result;
            }
            catch (TagLostException tle)
            {
                cmdErrorDesc += "Tag Lost Error: " + tle.Message;
                Console.WriteLine(cmdErrorDesc);
                Debug.WriteLine("return from tag lost");
                App.NfcError = true;
                return new byte[] { 0xFF, 0x01 };
            }
            catch (Android.Nfc.FormatException fe)
            {
                cmdErrorDesc += "Tag Format Error: " + fe.Message;
                Console.WriteLine(cmdErrorDesc);
                return new byte[] { 0xFF, 0x03 };
            }
            catch (SystemException sysEx)
            {
                cmdErrorDesc += "Error reading - sysEx: " + sysEx.Message;
                Console.WriteLine(cmdErrorDesc);
                return new byte[] { 0xFF, 0x04 };
            }
            catch (System.Exception errB)
            {
                cmdErrorDesc += "Error reading NFC: " + errB.Message;
                Console.WriteLine(cmdErrorDesc);
                return new byte[] { 0xFF, 0x05 };
            }

        }
    }
    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<INFCscan, nfcScan>();
        }
    }
}

