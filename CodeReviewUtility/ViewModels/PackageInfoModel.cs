using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeReviewUtility.Misc;
using Utility.Library.BusinessObjects;
using Utility.Model;

namespace CodeReviewUtility.ViewModels
{
    public class PackageInfoModel : ViewModelBase
    {
        private static string[] posibleSynopsis = { "DEV_TASK", "FIX_TASK", "MERGE_TASK", "REAPPLY_TASK", "POC_TASK" };

        public IEnumerable<string> PosibleSynopsis { get; private set; }
        public IEnumerable<string> PosibleCauses { get; private set; }

        public PackageInfoModel()
        {
            this.PosibleCauses = new List<string>();
            this.PosibleSynopsis = posibleSynopsis;
            this.Synopsis = "DEV_TASK";
        }

        public void FromModel(PackageInfo package)
        {
            this.Synopsis = package.Synopsis;
            this.Description = package.Description;
            this.Cause = package.Cause;
            this.TestsAddedOrChanged = package.TestsAddedOrChanged;
            this.TestsRun = package.TestsRun;
            this.AllTestsPass = package.AllTestsPass;
            this.SolutionBuilds = package.SolutionBuilds;

            this.ReadMeImpacted = package.ReadMeImpacted;
            this.DocumentationImpacted = package.DocumentationImpacted;
            this.InstallationImpacted = package.InstallationImpacted;

            this.SourceControlUrl = package.SourceControlServerPath;
        }

        public PackageInfo ToModel()
        {
            return new PackageInfo()
            {
                Synopsis = this.Synopsis,
                Description = this.Description,
                Cause = this.Cause,
                TestsAddedOrChanged = this.TestsAddedOrChanged,
                TestsRun = this.TestsRun,
                AllTestsPass = this.AllTestsPass,
                SolutionBuilds = this.SolutionBuilds,

                ReadMeImpacted = this.ReadMeImpacted,
                DocumentationImpacted = this.DocumentationImpacted,
                InstallationImpacted = this.InstallationImpacted
            };
        }


        public string Cause
        {
            get { return this.Get<string>("Cause"); }
            set { this.Set<string>("Cause", value); }
        }

        public string Description
        {
            get { return this.Get<string>("Description"); }
            set { this.Set<string>("Description", value); }
        }

        public string Synopsis
        {
            get { return this.Get<string>("Synopsis"); }
            set { this.Set<string>("Synopsis", value); }
        }

        public string Reviewers
        {
            get { return this.Get<string>("Reviewers"); }
            set { this.Set<string>("Reviewers", value); }
        }

        public string SourceControlUrl
        {
            get { return this.Get<string>("SourceControlUrl"); }
            set { this.Set<string>("SourceControlUrl", value); }
        }

        public string RestResultLocation
        {
            get { return this.Get<string>("RestResultLocation"); }
            set { this.Set<string>("RestResultLocation", value); }
        }

        public string TestsAddedOrChanged
        {
            get { return this.Get<string>("TestsAddedOrChanged"); }
            set { this.Set<string>("TestsAddedOrChanged", value); }
        }

        public string TestsRun
        {
            get { return this.Get<string>("TestsRun"); }
            set { this.Set<string>("TestsRun", value); }
        }

        public bool AllTestsPass
        {
            get { return this.Get<bool>("AllTestsPass"); }
            set { this.Set<bool>("AllTestsPass", value); }
        }

        public bool SolutionBuilds
        {
            get { return this.Get<bool>("SolutionBuilds"); }
            set { this.Set<bool>("SolutionBuilds", value); }
        }

        public bool InstallationImpacted
        {
            get { return this.Get<bool>("InstallationImpacted"); }
            set { this.Set<bool>("InstallationImpacted", value); }
        }

        public bool ReadMeImpacted
        {
            get { return this.Get<bool>("ReadMeImpacted"); }
            set { this.Set<bool>("ReadMeImpacted", value); }
        }

        public bool DocumentationImpacted
        {
            get { return this.Get<bool>("DocumentationImpacted"); }
            set { this.Set<bool>("DocumentationImpacted", value); }
        }
    }
}
