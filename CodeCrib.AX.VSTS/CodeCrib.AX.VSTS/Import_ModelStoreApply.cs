using CodeCrib.AX.BuildTasks;
using System.Management.Automation;


namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Import, "ModelStoreApply")]
    public class Import_ModelStoreApply : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Schema name to apply", Mandatory = true)]
        public string SchemaName;

        [Parameter(HelpMessage = "Folder path to an alternate axutil tool for use during model store export", Mandatory = false)]
        public string AxUtilBinaryFolderPath;

        protected override void ProcessRecord()
        {
            ModelStoreApplySchemaTask task = new ModelStoreApplySchemaTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile, AxUtilBinaryFolderPath, SchemaName);
            task.RunInAppDomain();
        }
    }
}
