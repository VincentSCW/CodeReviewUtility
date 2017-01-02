
using System;

namespace Utility.Library.Controls
{
    public class ListView : System.Windows.Controls.ListView
    {
        protected ListViewSorter Sorter { get; private set; }

        public ListView()
        {
            this.Sorter = new ListViewSorter(this);
        }
    }
}
