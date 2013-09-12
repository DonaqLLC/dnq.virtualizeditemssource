using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNQ.VirtualizedItemsSource
{
    partial class Operation<T>        
    {
        private class OperationResult<U>
            : IOperationResult<U>
        {
            public OperationResult(Operation<U> operation, U[] resultSet, IBounds newBounds, bool mustRefresh)
            {
                this.Operation = operation;
                NewBounds = newBounds;
                ResultSet = resultSet;
                MustRefresh = mustRefresh;
                Exception = null;
            }

            public OperationResult(Operation<U> operation, Exception exc)
            {
                Operation = operation;
                Exception = exc;
                NewBounds = null;
                ResultSet = null;
                MustRefresh = false;
            }

            public Exception Exception
            {
                get;
                private set;
            }

            public Operation<U> Operation
            {
                get;
                private set;
            }

            public U[] ResultSet
            {
                get;
                private set;
            }

            public IBounds NewBounds
            {
                get;
                private set;
            }

            public bool MustRefresh
            {
                get;
                private set;
            }
        }
    }
}
