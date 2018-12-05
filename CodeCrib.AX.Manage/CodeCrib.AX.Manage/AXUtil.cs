using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace CodeCrib.AX.Manage
{
    public class AXUtilCommand
    {
        public string ExecutablePath { get; set; }

        public static AXUtilCommand NewServerBinPath(string serverBinPath)
        {
            return new AXUtilCommand(Path.Combine(new[] { serverBinPath, "AXUtil.exe" }));
        }

        public AXUtilCommand(string executablePath)
        {
            ExecutablePath = executablePath;
        }
        public static void ExecuteCommand(string serverBinPath, string parameters, int timeOutMinutes)
        {
            AXUtilCommand axUtil = NewServerBinPath(serverBinPath);

            axUtil.Execute(parameters, timeOutMinutes);
        }

        public static AxUtilCommandContext StartCommand(string serverBinPath, string parameters)
        {
            AXUtilCommand axUtil = NewServerBinPath(serverBinPath);

            return axUtil.Start(parameters);
        }

        public void Execute(string parameters, int timeOutMinutes)
        {
            AxUtilCommandContext context = Start(parameters);

            Exception e = context.WaitForProcess(timeOutMinutes);

            if (e != null)
            {
                throw e;
            }
        }

        public AxUtilCommandContext Start(string parameters)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(ExecutablePath, parameters);
            processStartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            processStartInfo.WorkingDirectory = Path.GetDirectoryName(ExecutablePath);
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;

            AxUtilCommandContext context = new AxUtilCommandContext();
            context.StartProcess(processStartInfo);

            return context;
        }
    }

    public class AxUtilCommandContext
    {
        private StringBuilder _stdErr;
        private StringBuilder _stdOut;
        private ProcessStartInfo _procStartInfo;

        public string StandardError { get { return _stdErr.ToString(); } }
        public string StandardOutput { get { return _stdOut.ToString(); } }

        public Func<int, Exception> Delegate { get; set; }
        public Process Proc { get; set; }

        public void StartProcess(ProcessStartInfo processStartInfo)
        {
            _procStartInfo = processStartInfo;
            _stdErr = new StringBuilder();
            _stdOut = new StringBuilder();

            Proc = new Process();
            Proc.StartInfo = processStartInfo;
            Proc.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
            Proc.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);

            Proc.Start();

            Proc.BeginErrorReadLine();
            Proc.BeginOutputReadLine();
        }


        public Exception WaitForProcess(int timeOutMinutes)
        {
            Exception returnException = null;

            if (Proc.Id != 0)
            {
                if (timeOutMinutes > 0)
                {
                    if (!Proc.WaitForExit((int)new TimeSpan(0, timeOutMinutes, 0).TotalMilliseconds))
                    {
                        // Process is still running after the timeout has elapsed.

                        try
                        {
                            Proc.Kill();
                            returnException = new TimeoutException(string.Format("Client time out of {0} minutes exceeded", timeOutMinutes));
                        }
                        catch (Exception ex)
                        {
                            // Error trying to kill the process
                            returnException = new TimeoutException(string.Format("Client time out of {0} minutes exceeded, additionally an exception was encountered while trying to kill the client process (see innerexception)", timeOutMinutes), ex);
                        }
                    }
                }
                else
                {
                    Proc.WaitForExit();
                }
            }

            if (Proc.ExitCode != 0)
                returnException = new AXUtilException(String.Format("AXUtil with parameters '{0}' failed", _procStartInfo.Arguments), _stdErr.ToString(), _stdOut.ToString());

            return returnException;
        }

        private void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                _stdErr.Append(Environment.NewLine + " " + e.Data);
                Console.WriteLine("##vso[task.logissue type=error;]{0}", e.Data);
            }
        }

        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                _stdOut.Append(Environment.NewLine + " " + e.Data);
                Console.WriteLine(e.Data);
            }
        }
    }

    public class AXUtilException : Exception
    {
        private string _stdErr;
        private string _stdOut;

        public string StandardError { get { return _stdErr; } }
        public string StandardOutput { get { return _stdOut; } }

        public AXUtilException(string message, string stdErr, string stdOut, Exception innerException)
            : base(message, innerException)
        {
            _stdErr = stdErr;
            _stdOut = stdOut;
        }

        public AXUtilException(string message, string stdErr, string stdOut)
            : base(message)
        {
            _stdErr = stdErr;
            _stdOut = stdOut;
        }

        public override string ToString()
        {
            return String.Format("{0}\n{1}", Message, StandardError);
        }
    }
}
