using System;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class ModelStoreApplySchemaTask : BuildTask
    {
        protected string SchemaName;
        protected string AxUtilBinaryFolderPath;

        public ModelStoreApplySchemaTask()
        {
        }

        public ModelStoreApplySchemaTask(
            IBuildLogger buildLogger,
            string configurationFile,
            string axUtilBinaryFolderPath,
            string schemaName) : base(buildLogger, 0, configurationFile)
        {
            SchemaName = schemaName;
            AxUtilBinaryFolderPath = axUtilBinaryFolderPath;
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

            BuildLogger.LogInformation(string.Format("Initializing schema {0} in model store {1}{2} on server {3}", SchemaName, serverConfig.Database, (is60 ? "" : "_model"), serverConfig.DatabaseServer));
            modelStore.ModelStoreApplySchema(SchemaName);

        }
    }
}
