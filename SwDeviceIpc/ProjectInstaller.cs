using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace SwDeviceIpc
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private  ServiceProcessInstaller _installProcess;
        private  ServiceInstaller _installService;
        public ProjectInstaller()
        {
            InitializeComponent();

        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);
            _installProcess = new ServiceProcessInstaller();
            _installProcess.Account = ServiceAccount.NetworkService;

            _installService = new ServiceInstaller();
            _installService.StartType = ServiceStartMode.Automatic;

            //Remove built-in EventLogInstaller:
            _installService.Installers.Clear();

            Installers.Add(_installProcess);
            Installers.Add(_installService);
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);
        }
    }
}
