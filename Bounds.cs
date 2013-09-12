using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNQ.VirtualizedItemsSource
{
    /// <summary>   An object that represents a set of bounds on a virtualized view of items from the data source.  </summary>
    /// <remarks>   This object is immutable. </remarks>
    internal sealed class Bounds
        : IBounds
    {
        public Bounds(int lowBound, int highBound, int lowWater, int highWater)
        {
            LowBound = lowBound;
            HighBound = highBound;
            LowWater = lowWater;
            HighWater = highWater;
        }

        /// <summary>
        /// Read-only property that indicates the real low bound (index) for items;
        /// no items below this bound (index) can be shown until re-caching is complete..
        /// </summary>
        /// 
        /// <value> The low bound (index) for the items range represented by these bounds. </value>
        public int LowBound { get; internal set; }

        /// <summary>
        /// Read-only property that indicates the real high bound (index) for items;
        /// no items higher than this bound (index) can be shown until re-caching is complete..
        /// </summary>
        /// 
        /// <value> The high bound (index) for the items range represented by these bounds. </value>
        public int HighBound { get; internal set; }

        /// <summary>   
        /// Read-only property that indicates a safe low index, below which pro-active re-caching will occur
        /// but which is still higher than the real <see cref="LowBound"/>
        /// </summary>
        ///
        /// <value> A certain index between LowBound and HighBound below which a re-caching operation will occur. </value>
        /// <seealso cref="LowBound"/>
        public int LowWater { get; set; }

        /// <summary>   
        /// Read-only property that indicates a safe high index, beyond which pro-active re-caching will occur
        /// but which is still lower than the real <see cref="HighBound"/>
        /// </summary>
        ///
        /// <value> A certain index between LowBound and HighBound beyond which a re-caching operation will occur. </value>
        /// <seealso cref="HighBound"/>
        public int HighWater { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.String" /> representation of the current bounds object.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String" /> that represents the current set of bounds.
        /// </returns>
        public override string ToString()
        {
            return string.Format("[{0}:{1}(LW{2};HW{3})]", LowBound, HighBound, LowWater, HighWater);
        }

        /// <summary>   Shifts a given bounds object by a certain amount, either up or down, depending on the sign of <paramref name="shiftAmount"/> </summary>
        /// <param name="bounds">       The original bounds object to be shifted. </param>
        /// <param name="shiftAmount">  The shift amount. </param>
        ///
        /// <returns>   A new <see cref="Bounds"/> object. </returns>
        public static Bounds Shift(Bounds bounds, int shiftAmount)
        {
            return new Bounds(bounds.LowBound + shiftAmount, bounds.HighBound + shiftAmount, bounds.LowWater + shiftAmount, bounds.HighWater + shiftAmount);
        }
    }
}
