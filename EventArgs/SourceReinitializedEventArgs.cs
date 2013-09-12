using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNQ.VirtualizedItemsSource
{
    public class SourceReinitializedEventArgs
        : EventArgs
    {
        public SourceReinitializedEventArgs(int sourceSize, TimeSpan duration)
            : base()
        {
            Size = sourceSize;
            Duration = duration;
            StoredException = null;
        }

        public SourceReinitializedEventArgs(Exception storedException, TimeSpan duration)
            : base()
        {
            Size = -1;
            Duration = duration;
            StoredException = storedException;
        }

        public TimeSpan Duration
        {
            get;
            private set;
        }

        public Exception StoredException
        {
            get;
            private set;
        }

        public int Size
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return string.Format("SourceReinitialized: Size = {0}", Size);
        }
    }
}
