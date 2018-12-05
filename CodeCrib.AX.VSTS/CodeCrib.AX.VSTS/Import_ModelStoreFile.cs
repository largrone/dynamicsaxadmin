using CodeCrib.AX.BuildTasks;
using System.Management.Automation;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Import, "ModelStoreFile")]
    public class Import_ModelStoreFile : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Destination path to the exported model store", Mandatory = true)]
        public string ModelStoreFilePath;

        [Parameter(HelpMessage = "Import modelstore to schema", Mandatory = true)]
        public string SchemaName;

        [Parameter(HelpMessage = "How to handle ID conflicts", Mandatory = true)]
        [ValidateSet("Overwrite", "Reject")]
        public string IdConflict;

        [Parameter(HelpMessage = "Folder path to an alternate axutil tool for use during model store export", Mandatory = false)]
        public string AxUtilBinaryFolderPath;

        protected override void ProcessRecord()
        {
            ImportModelStoreTask task = new ImportModelStoreTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile, AxUtilBinaryFolderPath, IdConflict, SchemaName, ModelStoreFilePath);
            task.RunInAppDomain();
        }
    }
}
