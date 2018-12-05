using CodeCrib.AX.Client.AutoRun;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable] 
    public class RefreshServicesTask : ClientBuildTask
    {
        public RefreshServicesTask(
            IBuildLogger buildLogger,
            string configurationFile
            ) : base(buildLogger, 0, configurationFile)
        { 
        }

        public RefreshServicesTask(
            IBuildLogger buildLogger,
            int timeoutMinutes,
            string configurationFile) : base(buildLogger, timeoutMinutes, configurationFile)
        {
        }
            

        public RefreshServicesTask()
        {
        }

        public override void End()
        {
            AutoRunLogOutput.Output(BuildLogger, AutoRun, true);
        }

        public override Process Start()
        {
            Config.Client clientConfig = Deploy.Configs.GetClientConfig(ConfigurationFile);

            AutoRun = new AxaptaAutoRun() { ExitWhenDone = true, LogFile = string.Format(@"{0}\RefreshServicesLog-{1}.xml", Environment.ExpandEnvironmentVariables(clientConfig.LogDirectory), Guid.NewGuid()) };
            AutoRun.Steps.Add(new Client.AutoRun.Run() { Type = Client.AutoRun.RunType.@class, Name = "AifServiceGenerationManager", Method = "registerServices" });

            AutoRunFile = Path.Combine(Environment.GetEnvironmentVariable("temp"), string.Format("AutoRun-RefreshServices-{0}", Guid.NewGuid()));
            AxaptaAutoRun.SerializeAutoRun(AutoRun, AutoRunFile);

            BuildLogger.LogInformation(string.Format("Refreshing services"));
            BuildLogger.StoreLogFile(AutoRunFile);

            Process process = Client.Client.StartCommand(new Client.Commands.AutoRun() { ConfigurationFile = ConfigurationFile, Filename = AutoRunFile });

            return process;
        }
    }
}
