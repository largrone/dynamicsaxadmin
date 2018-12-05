using System;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class ImportModelStoreTask : BuildTask
    {
        protected string ModelStoreFilePath;
        protected string AxUtilBinaryFolderPath;
        protected string SchemaName;
        protected string IdConflict;

        public ImportModelStoreTask()
        {
        }

        public ImportModelStoreTask(
            IBuildLogger buildLogger,
            string configurationFile,
            string axUtilBinaryFolderPath,
            string idConflict,
            string schemaName,
            string modelStoreFilePath) : base(buildLogger, 0, configurationFile)
        {
            ModelStoreFilePath = modelStoreFilePath;
            AxUtilBinaryFolderPath = axUtilBinaryFolderPath;
            IdConflict = idConflict;
            SchemaName = schemaName;
        }
        public override void Run()
        {
            Config.Server serverConfig = Deploy.Configs.GetServerConfigRemote(ConfigurationFile);
            var is60 = serverConfig.AOSVersionOrigin.StartsWith("6.0", StringComparison.OrdinalIgnoreCase);

            Manage.ModelStore modelStore = null;
            if (is60)
            {
                modelStore = new Manage.ModelStore(serverConfig.DatabaseServer, serverConfig.Database, AxUtilBinaryFolderPath);
            }
            else
            {
                modelStore = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}_model", serverConfig.Database), AxUtilBinaryFolderPath);
            }

            Manage.IdConflict conflictEnum = Manage.IdConflict.Overwrite;
            switch (IdConflict)
            {
                case "Reject":
                    conflictEnum = Manage.IdConflict.Reject;
                    break;
                case "Overwrite":
                    conflictEnum = Manage.IdConflict.Overwrite;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("IdConflict", IdConflict, "Parameter IdConflict must be Overwrite or Reject");
            }

            BuildLogger.LogInformation(string.Format("Importing file {0} to model store {1}{2} in schema {3} on server {4}. Id conflict handling: {5}", ModelStoreFilePath, serverConfig.Database, (is60 ? "" : "_model"), SchemaName, serverConfig.DatabaseServer, conflictEnum));
            modelStore.ImportModelStore(ModelStoreFilePath, SchemaName, conflictEnum);

        }
    }
}
