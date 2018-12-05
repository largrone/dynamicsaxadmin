using System;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class ExportModelStoreTask : BuildTask
    {
        protected string ModelStoreFilePath;
        protected string AxUtilBinaryFolderPath;

        public ExportModelStoreTask()
        {
        }

        public ExportModelStoreTask(
            IBuildLogger buildLogger,
            string configurationFile,
            string axUtilBinaryFolderPath,
            string modelStoreFilePath) : base(buildLogger, 0, configurationFile)
        {
            ModelStoreFilePath = modelStoreFilePath;
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

            BuildLogger.LogInformation(string.Format("Exporting model store {0}{1} from server {2} to file {3}", serverConfig.Database, (is60?"":"_model"), serverConfig.DatabaseServer, ModelStoreFilePath));
            modelStore.ExportModelStore(ModelStoreFilePath);

        }
    }
}
