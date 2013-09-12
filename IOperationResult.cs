using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNQ.VirtualizedItemsSource
{
    /// <summary>   Interface that represents the result of a re-caching operation. </summary>
    /// <typeparam name="T">    Generic type parameter for the type of data items. </typeparam>
    internal interface IOperationResult<T>
    {
        Exception Exception
        {
            get;
        }

        DNQ.VirtualizedItemsSource.Operation<T> Operation
        {
            get;
        }

        T[] ResultSet
        {
            get;
        }

        IBounds NewBounds
        {
            get;
        }

        bool MustRefresh
        {
            get;
        }
    }
}
