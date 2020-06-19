using eDropletNFC.Resx;
using Prism.Commands;
using Prism.Mvvm;
using Syncfusion.SfBusyIndicator.XForms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eDropletNFC.ViewModels
{
    public class MasterPageViewModel : BindableBase
    {
        public string Title { get; private set; }
        public string homeTabTxt { get; private set; }
        public string toolsTabTxt { get; private set; }
        public string setupTabTxt { get; private set; }
        public string infoTabTxt { get; private set; }
        public MasterPageViewModel()
        {
            Title = "eDroplet NFC v.1";
            homeTabTxt = AppResources.toolbarHome;
            toolsTabTxt = AppResources.toolbarTools;
            infoTabTxt = AppResources.toolbarInfo;
            setupTabTxt = AppResources.toolbarSetup;
        }
    }
}
