using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;


namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Export, "ModelStoreFile")]
    public class Export_ModelStoreFile : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Destination path to the exported model store", Mandatory = true)]
        public string ModelStoreFilePath;

        [Parameter(HelpMessage = "Folder path to an alternate axutil tool for use during model store export", Mandatory = false)]
        public string AxUtilBinaryFolderPath;

        protected override void ProcessRecord()
        {
            ExportModelStoreTask task = new ExportModelStoreTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile, AxUtilBinaryFolderPath, ModelStoreFilePath);
            task.RunInAppDomain();
        }

    }
}
