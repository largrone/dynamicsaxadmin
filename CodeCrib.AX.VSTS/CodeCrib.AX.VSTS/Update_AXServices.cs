using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Update, "AXServices")]
    public class Update_AXServices : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        protected override void ProcessRecord()
        {
            RefreshServicesTask task = new RefreshServicesTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile);
            task.RunInAppDomain();
        }

    }
}
