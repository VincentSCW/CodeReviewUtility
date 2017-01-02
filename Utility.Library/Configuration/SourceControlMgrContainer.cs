using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Utility.Interface;

namespace Utility.Library.Configuration
{
    public class SourceControlMgrContainer
    {
        public static SourceControlMgrContainer Instance
        { 
            get; private set; 
        }

        private static object sync_obj;
        static SourceControlMgrContainer()
        {
            if (Instance != null)
                return;
            if (sync_obj == null)
                sync_obj = new object();

            lock (sync_obj)
            {
                Instance = new SourceControlMgrContainer();
            }
        }

        private List<string> nameOfTools;
        public List<string> NameOfTools
        {
            get
            {
                if (nameOfTools == null)
                {
                    nameOfTools = new List<string>();
                    var mgrSection = ConfigurationManager.GetSection("sourceControlManagerSection") as SourceControlMgrSection;
                    for (int i = 0; i < mgrSection.Managers.Count; i++)
                    {
                        nameOfTools.Add(mgrSection.Managers[i].MgrName);
                    }
                }
                return nameOfTools;
            }
        }

        private Dictionary<string, ISourceControlManager> managers = new Dictionary<string,ISourceControlManager>();
        public ISourceControlManager GetSouceControlManager(string type)
        {
            if (!managers.ContainsKey(type))
            {
                var mgrSection = ConfigurationManager.GetSection("sourceControlManagerSection") as SourceControlMgrSection;
                if (mgrSection.Managers.ContainsKey(type))
                    managers.Add(type, FindAndInvoke(mgrSection.Managers[type].Assembly));
            }

            return managers[type];
        }

        private ISourceControlManager FindAndInvoke(string assembly)
        {
            Assembly aby = Assembly.Load(assembly);
            var found = aby.GetExportedTypes().ToList().FirstOrDefault(a => a.GetInterfaces().Contains(typeof(ISourceControlManager)));
            if (found == null)
                throw new NotSupportedException(assembly);

            return aby.CreateInstance(found.FullName) as ISourceControlManager;
        }
    }
}
