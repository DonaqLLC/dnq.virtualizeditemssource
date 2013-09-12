using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNQ.VirtualizedItemsSource
{
    /// <summary>   Values that represents a type of re-caching operation. </summary>
    public enum OperationType
    {
        /// <summary>   An enum constant indicating that no re-caching operation is necessary or being performed. </summary>
        None = 0,

        /// <summary>   An enum constant indicating a cache-up (i.e. items with immediately higher indices that available in the current bounds). </summary>
        CacheUp = 1,
        
        /// <summary>   An enum constant indicating a cache-down (i.e. items with immediately lower indices that available in the current bounds). </summary>
        CacheDown = 2,
        
        /// <summary>   An enum constant indicating an arbitrary/random re-caching operation. </summary>
        Reposition = 3
    }
}
