using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNQ.VirtualizedItemsSource
{
    /// <summary>   Interface that represents the bounds of a virtualized view of items from the data source. </summary>
    public interface IBounds
    {
        /// <summary>
        /// Read-only property that indicates the real low bound (index) for items;
        /// no items below this bound (index) can be shown until re-caching is complete..
        /// </summary>
        /// 
        /// <value> The low bound (index) for the items range represented by these bounds. </value>
        int LowBound { get; }

        /// <summary>
        /// Read-only property that indicates the real high bound (index) for items;
        /// no items higher than this bound (index) can be shown until re-caching is complete..
        /// </summary>
        /// 
        /// <value> The high bound (index) for the items range represented by these bounds. </value>
        int HighBound { get; }

        /// <summary>   
        /// Read-only property that indicates a safe low index, below which pro-active re-caching will occur
        /// but which is still higher than the real <see cref="LowBound"/>
        /// </summary>
        ///
        /// <value> A certain index between LowBound and HighBound below which a re-caching operation will occur. </value>
        /// <seealso cref="LowBound"/>
        int LowWater { get; }


        /// <summary>   
        /// Read-only property that indicates a safe high index, beyond which pro-active re-caching will occur
        /// but which is still lower than the real <see cref="HighBound"/>
        /// </summary>
        ///
        /// <value> A certain index between LowBound and HighBound beyond which a re-caching operation will occur. </value>
        /// <seealso cref="HighBound"/>
        int HighWater { get; }
    }
}
