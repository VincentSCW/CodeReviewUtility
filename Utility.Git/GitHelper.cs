using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility.Library.Misc;

namespace Utility.Git
{
    internal class GitHelper
    {
        public static IList<string> ExecuteCmd(string arg, string folder)
        {
            string output;
            ProcessHelper.Execute("git", arg, folder, true, out output);

            return output.Split(new char[] { '\n' });
        }
    }
}
