using eDropletNFC.Resx;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eDropletNFC.ViewModels
{
    public class InfoPageViewModel : BindableBase
    {
        public string infoTabTxt { get; private set; }
        public InfoPageViewModel()
        {
            infoTabTxt = AppResources.toolbarInfo;

        }
    }
}
