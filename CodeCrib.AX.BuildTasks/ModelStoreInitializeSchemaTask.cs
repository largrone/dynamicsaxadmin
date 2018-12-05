using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class ModelStoreInitializeSchemaTask : BuildTask
    {
        protected string SchemaName;
        protected string AOSAccount;
        protected string AxUtilBinaryFolderPath;

        public ModelStoreInitializeSchemaTask()
        {
        }

        public ModelStoreInitializeSchemaTask(
            IBuildLogger buildLogger,
            string configurationFile,
            string axUtilBinaryFolderPath,
            string schemaName,
            string aosAccount) : base(buildLogger, 0, configurationFile)
        {
            SchemaName = schemaName;
            AOSAccount = aosAccount;
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

            BuildLogger.LogInformation(string.Format("Initializing schema {0} in model store {1}{2} for account {3} on server {4}", SchemaName, serverConfig.Database, (is60 ? "" : "_model"), AOSAccount, serverConfig.DatabaseServer));
            modelStore.InitializeAXModelStoreSchema(SchemaName, AOSAccount);
        }

    }
}
