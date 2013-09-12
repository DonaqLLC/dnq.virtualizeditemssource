using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNQ.VirtualizedItemsSource
{
    public class OperationErrorEventArgs
        : EventArgs
    {
        public OperationErrorEventArgs(Exception exc)
        {
            StoredException = exc;
        }

        public Exception StoredException
        {
            get;
            private set;
        }

        public bool ContinueExecution
        {
            get;
            set;
        }
    }
}
