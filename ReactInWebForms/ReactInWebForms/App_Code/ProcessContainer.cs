using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace ReactInWebForms.App_Code
{
    public class ProcessContainerKeepRunning
    {
        private readonly object lockObject = new object();
        private Process process;
        private List<string> receiver;
        private readonly string execFile;
        private readonly string workingDir;
        private readonly string arguments;
        private readonly Action<List<string>> fnProcessResult;
        public ProcessContainerKeepRunning(string execFile, string workingDir, string arguments, Action<List<string>> fnProcessResult)
        {
            this.execFile = execFile;
            this.workingDir = workingDir;
            this.arguments = arguments;
            this.fnProcessResult = fnProcessResult;
            startProcess();
        }
        public void RenewIfNecessary()
        {
            lock (lockObject)
            {
                var processNotRunning = true;
                try { processNotRunning = process.HasExited; }
                catch (InvalidOperationException) { }

                if (processNotRunning)
                {
                    EndProcess();
                    startProcess();
                }
            }
        }
        private void startProcess()
        {
            receiver = null;
            process = new Process();
            process.StartInfo.FileName = execFile;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = workingDir;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.OutputDataReceived += process_OutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
        }
        public List<string> GetLastOutput()
        {
            while (receiver == null)
                Thread.Sleep(200);
            return receiver;
        }
        public void EndProcess()
        {
            try { process.Kill(); } catch { }
        }

        private static List<string> convertOutput(string line)
        {
            if (string.IsNullOrEmpty(line))
                return new List<string>();

            var regData = Encoding.UTF8.GetString(Convert.FromBase64String(line));
            return new List<string>(regData.Split('\n'));
        }
        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var line = e.Data;
            if (line != null && line.StartsWith("FILE:", StringComparison.OrdinalIgnoreCase))
                lock (lockObject)
                {
                    if (receiver == null)
                        receiver = convertOutput(line.Substring(5));
                    else
                        fnProcessResult(receiver = convertOutput(line.Substring(5)));
                }
        }
    }

}