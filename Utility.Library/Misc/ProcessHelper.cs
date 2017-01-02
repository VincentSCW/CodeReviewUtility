using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Utility.Library.Misc
{
    /// <summary>
    /// Helper class to start a process and redirect the std to the caller
    /// </summary>
    public class ProcessHelper
    {
        private static IDictionary<int, Process> RunningProcess = new Dictionary<int, Process>();

        public static int Execute(string cmd, string[] args)
        {
            return Execute(cmd, IncludeQoutes(args));
        }

        public static int Execute(string cmd, string args)
        {
            string tmp;
            return Execute(cmd, args, string.Empty, false, out tmp);
        }

        public static int Execute(string cmd, string[] args, string workingDirectory, bool waitForExit)
        {
            string stdOutput;
            return Execute(cmd, IncludeQoutes(args), workingDirectory, waitForExit, out stdOutput);
        }

        public static int Execute(string cmd, string[] args, string workingDirectory, bool waitForExit, out string stdOutput)
        {
            return Execute(cmd, IncludeQoutes(args), workingDirectory, waitForExit, out stdOutput);
        }

        public static int Execute(string cmd, string args, string workingDirectory, bool waitForExit)
        {
            string stdOutput;
            return Execute(cmd, IncludeQoutes(args), workingDirectory, waitForExit, out stdOutput);
        }

        public static int Execute(string cmd, string args, string workingDirectory, bool waitForExit, out string stdOutput)
        {
            // Start the child process.
            Process p = new Process();
            stdOutput = string.Empty;
            try
            {
                // Redirect the output stream of the child process.
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = cmd;
                p.StartInfo.Arguments = args;
                p.StartInfo.WorkingDirectory = workingDirectory;
                p.Start();

                if (waitForExit)
                {
                    // Read the output stream first and then wait.            
                    stdOutput = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();

                    return p.ExitCode;
                }

                return 0;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message + "\nExecuting Command " + cmd + " " + args, "Error");
            }
            
            return 1;
        }

        public static string IncludeQoutes(params string[] args)
        {
            System.Text.StringBuilder arguments = new System.Text.StringBuilder();
            if (args != null)
            {
                foreach (string s in args)
                {
                    if (s.IndexOf(" ") != -1)  //we got a space in the path so wrap it in double qoutes
                    {
                        arguments.Append("\"");
                        arguments.Append(s);
                        arguments.Append("\"");
                    }
                    else
                    {
                        arguments.Append(s);
                    }

                    arguments.Append(" ");
                }
            }

            return arguments.ToString();
        }

    }
}
