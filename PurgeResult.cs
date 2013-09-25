using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNQ.VirtualizedItemsSource
{
    public class PurgeResult
    {
        internal PurgeResult(int numberOfItems)
        {
            NumberOrItemsPurged = numberOfItems;
        }

        public int NumberOrItemsPurged
        {
            get;
            private set;
        }

        public static readonly PurgeResult NotNeeded = new PurgeResult(0);
        public static readonly PurgeResult ErrorTimedout = new PurgeResult(-1);
    }
}
