using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DNQ.VirtualizedItemsSource
{
    internal partial class Operation<T>
    {
        private readonly object _synchObject = new object();
        private volatile bool _wasCancelled = false;
        private volatile bool _wasSuccessful = false;
        private static int _operationsCount = 0;
        private Action<OperationResult<T>> _callback;
        private CancellationTokenSource _cancellationSource;

        public Operation(OperationType opType, int start, int end, int span, IBounds originalBounds, int originalTotalCount, Action<IOperationResult<T>> callback)
        {
            OperationType = opType; Start = start; End = end; Span = span;
            OriginalBounds = originalBounds;
            OriginalTotalCount = originalTotalCount;
            _callback = callback;

            _cancellationSource = new CancellationTokenSource();

            OperationNumber = Interlocked.Increment(ref _operationsCount);            
        }

        public int OperationNumber { get; private set; }

        public OperationType OperationType { get; private set; }
        public int Start { get; private set; }
        public int End { get; private set; }
        public int Span { get; private set; }
        public IBounds OriginalBounds { get; private set; }
        public int OriginalTotalCount { get; private set; }
        

        public bool WasCancelled
        {
            get
            {
                lock (_synchObject)
                {
                    return _wasCancelled;
                }
            }
        }

        public bool WasSuccessful
        {
            get
            {
                lock (_synchObject)
                {
                    return _wasSuccessful;
                }
            }
        }

        public void Cancel()
        {
            lock (_synchObject)
            {
                _wasCancelled = true;
                _cancellationSource.Cancel();
            }
        }

        public CancellationToken CancellationToken
        {
            get
            {
                return _cancellationSource.Token;
            }
        }

        public void SetComplete(T[] array, IBounds b, bool mustRefresh)
        {
            lock (_synchObject)
            {
                _wasSuccessful = true;
            }
            if (_callback != null)
            {
                OperationResult<T> opResult = new OperationResult<T>(this, array, b, mustRefresh);
                _callback(opResult);
            }
        }

        public void SetComplete(Exception exc)
        {
            lock (_synchObject)
            {
                _wasSuccessful = false;
            }
            if (_callback != null)
            {
                OperationResult<T> opResult = new OperationResult<T>(this, exc);
                _callback(opResult);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} #{1}: From {2} to {3}, Span = {4}", OperationType, OperationNumber, Start, End, Span);
        }
    }
}
