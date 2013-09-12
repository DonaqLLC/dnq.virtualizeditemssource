using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNQ.VirtualizedItemsSource
{
    public class RepositionCompleteEventArgs
        : EventArgs
    {
        public RepositionCompleteEventArgs(bool refreshRequired)
        {
            RefreshRequired = refreshRequired;
            StoredException = null;
        }

        public RepositionCompleteEventArgs(Exception storedExc)
        {
            StoredException = storedExc;
            RefreshRequired = false;
        }

        public Exception StoredException
        {
            get;
            private set;
        }

        public bool RefreshRequired
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return string.Format("RepositionCompleteEventArgs: Refresh = {0}", RefreshRequired);
        }
    }
}
