using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Model
{
    /// <summary>
    /// Details to be included in the packge that is send out for review
    /// </summary>
    public class PackageInfo
    {
        public string SourceControlServerPath { get; set; }
        public string SourceFolder { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public List<string> FileNames { get; set; }

        public string Cause { get; set; }
        public string Synopsis { get; set; }
        public string TestsAddedOrChanged { get; set; }
        public string TestsRun { get; set; }
        public bool AllTestsPass { get; set; }
        public bool SolutionBuilds { get; set; }

        public bool InstallationImpacted { get; set; }
        public bool ReadMeImpacted { get; set; }
        public bool DocumentationImpacted { get; set; }
    }
}
