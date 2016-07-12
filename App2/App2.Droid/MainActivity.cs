using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;
using Xamarin.Forms;
using System.Collections.Generic;

namespace App2.Droid
{
    [Activity(Label = "App2", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity, EMDKManager.IEMDKListener
    {
        private EMDKManager emdkManager = null;
        private BarcodeManager barcodeManager = null;
        private Scanner scanner = null;

        public string ScannerStatus { get; set; }

        public void OnClosed()
        {
            ScannerStatus = "Status: EMDK Open failed unexpectedly. ";

            if (emdkManager != null)
            {
                emdkManager.Release();
                emdkManager = null;
            }
        }

        public void OnOpened(EMDKManager emdkManager)
        {
            ScannerStatus = "Status: EMDK Opened successfully ...";
            this.emdkManager = emdkManager;

            InitScanner();
        }

        protected override void OnCreate(Bundle bundle)
        {
            

            
            base.OnCreate(bundle);


            EMDKResults results = EMDKManager.GetEMDKManager(Android.App.Application.Context, this);

            if (results.StatusCode != EMDKResults.STATUS_CODE.Success)
            {
                ScannerStatus = "Status: EMDKManager object creation failed ...";
            }
            else
            {
                ScannerStatus = "Status: EMDKManager object creation succeeded ...";
            }

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        protected override void OnResume()
        {
            base.OnResume();
            InitScanner();
        }

        protected override void OnPause()
        {
            base.OnPause();
            DeinitScanner();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (emdkManager != null)
            {
                emdkManager.Release();
                emdkManager = null;
            }
        }

        public void OnScanEvent(object sender, Scanner.DataEventArgs e)
        {
            ScanDataCollection scanDataCollection = e.P0;

            if ((scanDataCollection != null) && (scanDataCollection.Result == ScannerResults.Success))
            {
                IList<ScanDataCollection.ScanData> scanData = scanDataCollection.GetScanData();

                foreach (ScanDataCollection.ScanData data in scanData)
                {
                    string dataString = data.Data;
                    //var app = App.Current;
                    //var mp = App.Current.MainPage;
                    MessagingCenter.Send<Xamarin.Forms.Application, string>(App.Current, "InitialScan", dataString);
                }
            }
        }

        public void OnStatusEvent(object sender, Scanner.StatusEventArgs e)
        {
            // EMDK: The status will be returned on multiple cases. Check the state and take the action.
            StatusData.ScannerStates state = e.P0.State;

            if (state == StatusData.ScannerStates.Idle)
            {
                ScannerStatus = "Scanner is idle and ready to submit read.";
                try
                {
                    if (scanner.IsEnabled && !scanner.IsReadPending)
                    {
                        scanner.Read();
                    }
                }
                catch (ScannerException e1)
                {
                    ScannerStatus = e1.Message;
                }
            }
            if (state == StatusData.ScannerStates.Waiting)
            {
                ScannerStatus = "Waiting for Trigger Press to scan";
            }
            if (state == StatusData.ScannerStates.Scanning)
            {
                ScannerStatus = "Scanning in progress...";
            }
            if (state == StatusData.ScannerStates.Disabled)
            {
                ScannerStatus = "Scanner disabled";
            }
            if (state == StatusData.ScannerStates.Error)
            {
                ScannerStatus = "Error occurred during scanning";
            }
        }

        
        private void InitScanner()
        {
            if (emdkManager != null)
            {
                if (barcodeManager == null)
                {
                    try
                    {
                        // Get the feature object such as BarcodeManager object for accessing the feature.
                        barcodeManager = (BarcodeManager)emdkManager.GetInstance(EMDKManager.FEATURE_TYPE.Barcode);

                        scanner = barcodeManager.GetDevice(BarcodeManager.DeviceIdentifier.Default);

                        if (scanner != null)
                        {
                            // Attahch the Data Event handler to get the data callbacks.
                            scanner.Data += OnScanEvent;

                            // Attach Scanner Status Event to get the status callbacks.
                            scanner.Status += OnStatusEvent;

                            scanner.Enable();

                            // EMDK: Configure the scanner settings
                            ScannerConfig config = scanner.GetConfig();
                            config.SkipOnUnsupported = ScannerConfig.SkipOnUnSupported.None;
                            config.ScanParams.DecodeLEDFeedback = false;
                            config.ReaderParams.ReaderSpecific.ImagerSpecific.PickList = ScannerConfig.PickList.Enabled;
                            config.DecoderParams.Code39.Enabled = false;
                            config.DecoderParams.Code128.Enabled = true;
                            scanner.SetConfig(config);
                        }
                        else
                        {
                            // displayStatus("Failed to enable scanner.\n");
                        }
                    }
                    catch (ScannerException e)
                    {
                        // displayStatus("Error: " + e.Message);
                    }
                    catch (Exception ex)
                    {
                        // displayStatus("Error: " + ex.Message);
                    }
                }
            }
        }

        private void DeinitScanner()
        {
            if (emdkManager != null)
            {
                if (scanner != null)
                {
                    try
                    {
                        scanner.Data -= OnScanEvent;
                        scanner.Disable();
                    }
                    catch (ScannerException e)
                    {
                        // Log.Debug(this.Class.SimpleName, "Exception:" + e.Result.Description);
                    }
                }

                if (barcodeManager != null)
                {
                    emdkManager.Release(EMDKManager.FEATURE_TYPE.Barcode);
                }
                barcodeManager = null;
                scanner = null;
            }
        }
    }
}

