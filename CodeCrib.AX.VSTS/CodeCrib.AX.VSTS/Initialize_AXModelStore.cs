using CodeCrib.AX.BuildTasks;
using System.Management.Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Initialize, "AXModelStore")]
    public class Initialize_AXModelStore : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Schema name to apply", Mandatory = true)]
        public string SchemaName;

        [Parameter(HelpMessage = "AOS Account", Mandatory = true)]
        public string AOSAccount;

        [Parameter(HelpMessage = "Folder path to an alternate axutil tool for use during model store export", Mandatory = false)]
        public string AxUtilBinaryFolderPath;

        protected override void ProcessRecord()
        {
            ModelStoreInitializeSchemaTask task = new ModelStoreInitializeSchemaTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile, AxUtilBinaryFolderPath, SchemaName, AOSAccount);
            task.RunInAppDomain();
        }

    }
}
