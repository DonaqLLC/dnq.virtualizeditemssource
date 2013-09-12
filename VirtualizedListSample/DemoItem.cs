using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualizedListSample
{
    public class DemoItem
    {
        public DemoItem(string name, string details)
        {
            Name = name;
            Details = details;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Details
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
