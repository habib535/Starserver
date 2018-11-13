using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;

namespace StarServer
{
    public static class Processor
    {
        public static void ExecuteCommand(string fileName, string arguments = null)
        {
            //StringCollection info = new StringCollection();
            //StringCollection errors = new StringCollection();
            string allErrors = string.Empty;
            ProcessStartInfo startInfo = null;
            var proc = new Process();
            if (string.IsNullOrWhiteSpace(arguments))
            {
                startInfo = new ProcessStartInfo(fileName);
            }
            else
            {
                startInfo = new ProcessStartInfo(fileName, arguments);
            }
            //startInfo.UseShellExecute = false;
            //startInfo.CreateNoWindow = true;
            //startInfo.RedirectStandardError = true;
            //startInfo.RedirectStandardOutput = true;
            proc.StartInfo = startInfo;
            proc.Start();

            proc.WaitForExit();

            //allErrors = proc.StandardError.ReadToEnd();
            //return allErrors;
        }
    }
}
