using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DNQ.VirtualizedItemsSource
{
    /// <summary>   A generic virtualized item source that can be used to provide a linear (sequential) view into a large data set. </summary>
    ///
    /// <typeparam name="T">    Generic type parameter for the type of data items in the data set. </typeparam>
    public class VirtualizedItemSource<T>
    {
        private ReaderWriterLockSlim _internalReadWriteLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private ManualResetEvent _evtReadyToDisplay = new ManualResetEvent(true);

        private T[] _cachedItems;
        private IBounds _currentBounds;
        private int _totalCount = 0;                            // this indicates the reported total number of items (as reported by the _countSourceElementsFunction
        private Operation<T> _currentOperation = null;        

        private Func<int> _countSourceElementsFunction;
        private Func<int, int, CancellationToken, IEnumerable<T>> _retrieveSourceElementsQueryFunction;
        private Action<int, string> _logFunction;

        /// <summary>   Creates a new instance of a virtualized items source. </summary>
        ///
        /// <param name="span">                                 The number of items to make available ay any one point in the linear/sequential cache. </param>
        /// <param name="countSourceElements">                  A delegate that will be invoked whenever this object needs to obtain the total number of items available in the source data set. </param>
        /// <param name="retrieveSourceElementsQueryFunction">  A delegate that will be invoked by this object to obtain items from the source data set. </param>
        /// <param name="logFunction">                          A delegate that will be invoked by this object to log certain events. </param>
        public VirtualizedItemSource(int span, Func<int> countSourceElements, Func<int, int, CancellationToken, IEnumerable<T>> retrieveSourceElementsQueryFunction, Action<int, string> logFunction)
        {
            Span = span;
            _countSourceElementsFunction = countSourceElements;
            _retrieveSourceElementsQueryFunction = retrieveSourceElementsQueryFunction;
            _logFunction = logFunction;
        }

        /// <summary>   Event raised after this virtualized items source is reinitialized. 
        ///             It indiactes the success or failure of the operation. </summary>
        public event EventHandler<SourceReinitializedEventArgs> SourceReinitialized;
        
        /// <summary>   Event raised after a repositioning occures in this virtualized items source.
        ///             It indicates the success or failure of the operation. </summary>
        /// <seealso cref="NotifyRepositionRequired"/>
        public event EventHandler<RepositionCompleteEventArgs> SourceRepositioned;

        /// <summary>
        /// Event raised when a reposition is required in order to be able to retrieve a requested item. 
        /// </summary>
        /// <remarks>
        /// The event is raised in order to notify consumers that a repositioning is under way but no other information
        /// is provided, nor are consumers given the ability to alter the subsequent repositioning in any way.
        /// 
        /// Consumers may use this event to signal in the user interface that a repositions is taking place. 
        /// The <see cref="SourceRepositioned"/> may then be used to restore the user interface to the normal state.
        /// </remarks>
        /// <seealso cref="SourceRepositioned"/>
        public event EventHandler<EventArgs> NotifyRepositionRequired;

        /// <summary>   Event raised whenever an error occurs. </summary>
        public event EventHandler<OperationErrorEventArgs> NotifyErrorOccured;

        /// <summary>   Gets the span of elements that are expected to be available at any one time in the virtualized items source.
        ///             This value is set in the constructor.</summary>
        /// <remarks>   The span should be large enough to contain all elements that would be visible at one time (i.e. if the 
        ///             list can display 100 visisble items, the span should be at least 100) with a good number being twice 
        ///             or three times that.</remarks>
        /// <value> The span. </value>
        public int Span
        {
            get;
            private set;
        }

        /// <summary>   Gets the total number of items in the source data set. </summary>
        ///
        /// <value> Integer, total number of data items in the source data set. </value>
        public int TotalCount
        {
            get
            {
                bool lockAcquired = false;
                try
                {
                    if (!_internalReadWriteLock.IsReadLockHeld)
                    {                        
                        _internalReadWriteLock.EnterReadLock();
                        lockAcquired = true;
                    }

                    return _totalCount;
                }
                finally
                {
                    if (lockAcquired)
                    {
                        _internalReadWriteLock.ExitReadLock();
                    }
                }
            }
        }

        /// <summary>   Gets an item from the source data set at the given index. </summary>
        /// <remarks>   This method returns an item from the source data set and handles all the necessary caching operations
        ///             in order to make this item available. 
        ///             
        ///             Note that the index of the item to be retrieved is relative to the source data set range, not the
        ///             virtualized item source internal caching.
        /// </remarks>
        /// 
        /// <param name="index">    Zero-based index of the data item to be retrieved from the source data set. </param>
        /// <param name="item">     [out] The data item that is returned. </param>
        /// <seealso cref="EnsureItemsAvailable"/>
        /// <returns>   True if the item can be retrieved, false otherwise. </returns>
        public bool GetItem(int index, out T item)
        {
            item = default(T);
            bool doit = false;
            int failCount = 0;

            do
            {
                if (!EnsureItemsAvailable(index, index))
                    failCount++;
                doit = false;

                bool lockAcquired = false;
                try
                {
                    if (!_internalReadWriteLock.IsReadLockHeld)
                    {
                        _internalReadWriteLock.EnterReadLock();
                        lockAcquired = true;
                    }
                    
                    int computedIndex = index - _currentBounds.LowBound;

                    if ((index >= 0 && index < _currentBounds.LowBound) || (index >= _currentBounds.HighBound && index < _totalCount))
                    {
                        doit = true;
                    }
                    if (!doit && (computedIndex >= 0 && computedIndex < _cachedItems.Length))
                    {
                        item = _cachedItems[computedIndex];
                    }
                }
                finally
                {
                    if (lockAcquired)
                    {
                        _internalReadWriteLock.ExitReadLock();
                    }
                }
            } while (doit && failCount < 3);

            return item != null;
        }

        public WaitHandle ReinitializeSource(int expectedCount)
        {
            _evtReadyToDisplay.Reset();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            ThreadPool.QueueUserWorkItem((WaitCallback)delegate
            {
                T[] newItemsArray = null;
                int newHBound = 0;

                Exception storedException = null;
                try
                {
                    if (expectedCount == -1)
                    {
                        expectedCount = _countSourceElementsFunction();
                    }

                    newHBound = Math.Min(Span, expectedCount);
                    newItemsArray = _retrieveSourceElementsQueryFunction(0, newHBound, CancellationToken.None).ToArray();

                    bool lockAcquired = false;
                    try
                    {
                        _internalReadWriteLock.EnterWriteLock();
                        lockAcquired = true;

                        _currentBounds = new Bounds(0, newHBound, 0, newHBound);
                        _totalCount = expectedCount;
                        _cachedItems = newItemsArray;
                    }
                    finally
                    {
                        if (lockAcquired)
                        {
                            _internalReadWriteLock.ExitWriteLock();
                        }
                    }

                    sw.Stop();                    
                }
                catch(Exception exc)
                {
                    storedException = exc;
                }
                finally
                {
                    if (SourceReinitialized != null)
                    {
                        if (storedException == null)
                        {
                            SourceReinitialized(this, new SourceReinitializedEventArgs(expectedCount, sw.Elapsed));
                        }
                        else
                        {
                            SourceReinitialized(this, new SourceReinitializedEventArgs(storedException, sw.Elapsed));
                        }
                    }

                    if (storedException != null && NotifyErrorOccured != null)
                    {
                        NotifyErrorOccured(this, new OperationErrorEventArgs(storedException));
                    }
                }

                _evtReadyToDisplay.Set();
            });

            return _evtReadyToDisplay;
        }

        /// <summary>   Ensures that items between <paramref name="startIndex"> and <paramref name="endIndex"> are available/cached. </summary>
        ///
        /// <param name="startIndex">    The start index of the first item. </param>
        /// <param name="endIndex">      The end index of the last item. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        public bool EnsureItemsAvailable(int startIndex, int endIndex)
        {            
            OperationType requiredOperation = OperationType.None;
            Operation<T> scheduledOperation = null;

            bool lockAcquired = false;
            try
            {
                if (!_internalReadWriteLock.IsReadLockHeld)
                {
                    _internalReadWriteLock.EnterReadLock();
                    lockAcquired = true;
                }

                if ((startIndex >= 0 && startIndex < _currentBounds.LowBound) || (startIndex >= _currentBounds.HighBound && startIndex < _totalCount) || (endIndex >= 0 && endIndex < _currentBounds.LowBound) || (endIndex >= _currentBounds.HighBound && endIndex < _totalCount))
                {
                    requiredOperation = OperationType.Reposition;
                }
                else if (startIndex < _currentBounds.LowWater && _currentBounds.LowBound > 0)
                {
                    requiredOperation = OperationType.CacheDown;
                }
                else if (endIndex > _currentBounds.HighWater && _currentBounds.HighBound < _totalCount)
                {
                    requiredOperation = OperationType.CacheUp;
                }
                else
                {
                    requiredOperation = OperationType.None;
                }

                if (_currentOperation != null)                                  // if there's something going on now..
                {
                    if (_currentOperation.OperationType == requiredOperation)       // and it's exactly what we would like it to be..
                        requiredOperation = OperationType.None;                     //    no operation is required anymore
                    else if (requiredOperation != OperationType.None)               // but if it's not what we wanted it to be and we wanted anything but "NONE"
                        requiredOperation = OperationType.Reposition;               //    ... then we need to do a reposition (cancel whatever is going on and REPOSITION)
                }
            }
            finally
            {
                if (lockAcquired)
                {
                    _internalReadWriteLock.ExitReadLock();
                }
            }

            if (requiredOperation != OperationType.None)                   // if we got here and we need to do something..
            {
                if (requiredOperation == OperationType.CacheUp)
                {
                    scheduledOperation = ScheduleCacheUp(startIndex, endIndex, Span);
                }
                else if (requiredOperation == OperationType.CacheDown)
                {
                    scheduledOperation = ScheduleCacheDown(startIndex, endIndex, Span);
                }
                else if (requiredOperation == OperationType.Reposition)
                {
                    scheduledOperation = ScheduleReposition(startIndex, endIndex, Span);
                }
            }
            
            _evtReadyToDisplay.WaitOne();

            if (scheduledOperation != null)
            {
                return scheduledOperation.WasSuccessful;
            }

            return true;
        }

        #region Private Methods

        private Operation<T> ScheduleReposition(int start, int end, int span)
        {
            if (NotifyRepositionRequired != null)
            {
                NotifyRepositionRequired(this, EventArgs.Empty);
            }

            try
            {
                _internalReadWriteLock.EnterWriteLock();
                if (_currentOperation != null)
                {
                    _currentOperation.Cancel();
                }
                _currentOperation = new Operation<T>(OperationType.Reposition, start, end, span, _currentBounds, _totalCount, RepositionComplete);

                _evtReadyToDisplay.Reset();
                ThreadPool.QueueUserWorkItem((WaitCallback)RepositionItemsSource, _currentOperation);

                return _currentOperation;
            }
            finally
            {
                if(_internalReadWriteLock.IsWriteLockHeld)
                    _internalReadWriteLock.ExitWriteLock();
            }
        }

        private Operation<T> ScheduleCacheDown(int start, int end, int span)
        {
            try
            {
                _internalReadWriteLock.EnterWriteLock();
                if (_currentOperation != null)
                {
                    if (_logFunction != null)
                    {
                        _logFunction(1, string.Format("VirtualizedList Error: Cannot schedule a Cache Down operation when another operation is still pending.."));
                    }
                    throw new InvalidOperationException("Cannot schedule a Cache Down operation when another operation is still pending..");
                }

                _currentOperation = new Operation<T>(OperationType.CacheDown, start, end, span, _currentBounds, _totalCount, CacheUpOrDownComplete);
                ThreadPool.QueueUserWorkItem((WaitCallback)CacheDownItemsSource, _currentOperation);

                return _currentOperation;
            }
            finally
            {
                if (_internalReadWriteLock.IsWriteLockHeld)
                    _internalReadWriteLock.ExitWriteLock();
            }
        }

        private Operation<T> ScheduleCacheUp(int start, int end, int span)
        {
            try
            {
                _internalReadWriteLock.EnterWriteLock();
                if (_currentOperation != null)
                {
                    if (_logFunction != null)
                    {
                        _logFunction(1, string.Format("VirtualizedList Error: Cannot schedule a Cache Up operation when another operation is still pending.."));
                    }
                    throw new InvalidOperationException("Cannot schedule a Cache Up operation when another operation is still pending..");
                }

                _currentOperation = new Operation<T>(OperationType.CacheUp, start, end, span, _currentBounds, _totalCount, CacheUpOrDownComplete);
                ThreadPool.QueueUserWorkItem((WaitCallback)CacheUpItemsSource, _currentOperation);

                return _currentOperation;
            }
            finally
            {
                if (_internalReadWriteLock.IsWriteLockHeld)
                    _internalReadWriteLock.ExitWriteLock();
            }
        }

        private void RepositionComplete(IOperationResult<T> operationResult)
        {            
            try{
                _internalReadWriteLock.EnterWriteLock();

                if (!ReferenceEquals(operationResult.Operation, _currentOperation))
                {
                    if (_logFunction != null)
                    {
                        _logFunction(6, string.Format("Cache REPOSITIONING #{0} COMPLETE BUT DISCARDING BECAUSE IT WAS NOT VALID ANYMORE..", operationResult.Operation.OperationNumber));
                    }
                    return;
                }

                _currentOperation = null;

                if (operationResult.ResultSet != null && operationResult.NewBounds != null)
                {
                    _currentBounds = operationResult.NewBounds;
                    _cachedItems = operationResult.ResultSet;

                    if (_logFunction != null)
                    {
                        _logFunction(6, string.Format("Cache REPOSITIONING #{0} COMPLETE .. UPDATED BOUNDS / ITEMS SOURCE.. {1} READY..", operationResult.Operation.OperationNumber, _currentBounds));
                    }                    
                }
                else
                {
                    if (_logFunction != null)
                    {
                        _logFunction(6, string.Format("+++> Cache REPOSITIONING #{0} COMPLETE .. NOT UPDATED BOUNDS / ITEMS SOURCE.. {1} READY..", operationResult.Operation.OperationNumber, _currentBounds));
                    }
                }
            }
            finally
            {
                if(_internalReadWriteLock.IsWriteLockHeld)
                    _internalReadWriteLock.ExitWriteLock();
            }            

            if (SourceRepositioned != null)
            {
                if (operationResult.Exception == null)
                {
                    SourceRepositioned(this, new RepositionCompleteEventArgs(operationResult.MustRefresh));
                }
                else
                {
                    SourceRepositioned(this, new RepositionCompleteEventArgs(operationResult.Exception));
                }
            }

            _evtReadyToDisplay.Set();

            if (operationResult.Exception != null && NotifyErrorOccured != null)
            {
                NotifyErrorOccured(this, new OperationErrorEventArgs(operationResult.Exception));
            }
        }

        private void CacheUpOrDownComplete(IOperationResult<T> operationResult)
        {
            var originalOperation = operationResult.Operation;
            bool mustRepositionAfterThis = false;

            try{            
                _internalReadWriteLock.EnterWriteLock();

                if (!ReferenceEquals(originalOperation, _currentOperation))
                    return;

                _currentOperation = null;

                if (operationResult.ResultSet != null && operationResult.NewBounds != null && operationResult.MustRefresh == false)
                {
                    T[] tmp = _cachedItems;
                    _cachedItems = new T[Math.Min(2 * originalOperation.Span, operationResult.ResultSet.Length + tmp.Length)];
                    int resultSetLen = operationResult.ResultSet.Length;
                    int preserveCount = _cachedItems.Length - resultSetLen;

                    if (originalOperation.OperationType == OperationType.CacheDown)
                    {
                        if (resultSetLen + tmp.Length <= _cachedItems.Length)
                            preserveCount = tmp.Length;
                        Array.Copy(operationResult.ResultSet, 0, _cachedItems, 0, resultSetLen);
                        Array.Copy(tmp, 0, _cachedItems, resultSetLen, preserveCount);
                    }
                    else if (originalOperation.OperationType == OperationType.CacheUp)
                    {
                        Array.Copy(tmp, tmp.Length - preserveCount, _cachedItems, 0, preserveCount);
                        Array.Copy(operationResult.ResultSet, 0, _cachedItems, preserveCount, resultSetLen);
                    }

                    _currentBounds = operationResult.NewBounds;
                }
                else if (operationResult.MustRefresh)   // UN COMMENT THIS WHEN DONE WITH DEBUGGING THE REPOSITIONING PROBLEMS..
                {
                    mustRepositionAfterThis = true;
                }                
            }
            finally
            {
                if (_internalReadWriteLock.IsWriteLockHeld)
                    _internalReadWriteLock.ExitWriteLock();
            }

            if (operationResult.Exception != null && NotifyErrorOccured != null)
            {
                var opErrArgs = new OperationErrorEventArgs(operationResult.Exception);
                NotifyErrorOccured(this, opErrArgs);
                mustRepositionAfterThis = opErrArgs.ContinueExecution;
            }

            if(mustRepositionAfterThis)
                ScheduleReposition(originalOperation.Start, originalOperation.End, originalOperation.Span);
        }

        private void CacheDownItemsSource(object boxedOperation)
        {
            Operation<T> op = boxedOperation as Operation<T>;
            Exception storedException = null;
            bool mustRefresh = false;
            T[] resultSetArray = null;
            int newLBound = op.OriginalBounds.LowBound; int newHBound = op.OriginalBounds.HighBound;
            int newLWater = op.OriginalBounds.LowWater; int newHWater = op.OriginalBounds.HighWater;
            string logMessage = string.Empty;

            try
            {
                int totCount = _countSourceElementsFunction();
                if (totCount != op.OriginalTotalCount)
                {
                    mustRefresh = true;
                    return;
                }

                logMessage = string.Format("Caching DOWN #{0}: Original {1} ..", op.OperationNumber, op.OriginalBounds);

                int howMany = op.Span / 2;

                if (op.OriginalBounds.LowBound < howMany)
                    howMany = op.OriginalBounds.LowBound;

                if (howMany > 0)
                {
                    resultSetArray = _retrieveSourceElementsQueryFunction(op.OriginalBounds.LowBound - howMany, howMany, op.CancellationToken).ToArray();

                    logMessage = string.Format("{0} adding {1} records .. ", logMessage, howMany);

                    var resultingItemsSourceSize = Math.Min(2 * op.Span, resultSetArray.Length + (op.OriginalBounds.HighBound - op.OriginalBounds.LowBound));

                    newLBound = op.OriginalBounds.LowBound - howMany;
                    newLWater = op.OriginalBounds.LowWater - howMany;
                    newHBound = newLBound + resultingItemsSourceSize;
                    newHWater = op.OriginalBounds.HighWater - howMany;
                }
            }
            catch (Exception exc)
            {
                storedException = exc;
            }
            finally
            {
                if (!op.WasCancelled)
                {
                    if (storedException != null)
                    {
                        if (_logFunction != null)
                        {
                            _logFunction(6, string.Format("{0} --> Failed!", logMessage));
                        }                        
                        op.SetComplete(storedException);
                    }
                    else
                    {
                        IBounds newBounds = new Bounds(newLBound, newHBound, newLWater, newHWater);
                        if (_logFunction != null)
                        {
                            _logFunction(6, string.Format("{0} --> New {1}", logMessage, newBounds));
                        }
                        op.SetComplete(resultSetArray, newBounds, mustRefresh);
                    }
                }
            }
        }

        private void CacheUpItemsSource(object boxedOperation)
        {
            Operation<T> op = boxedOperation as Operation<T>;
            Exception storedException = null;
            bool mustRefresh = false;
            T[] resultSetArray = null;
            int newLBound = op.OriginalBounds.LowBound; int newHBound = op.OriginalBounds.HighBound;
            int newLWater = op.OriginalBounds.LowWater; int newHWater = op.OriginalBounds.HighWater;
            string logMessage = string.Empty;

            try
            {
                int totCount = _countSourceElementsFunction();
                if (totCount != op.OriginalTotalCount)
                {
                    mustRefresh = true;
                    return;
                }

                logMessage = string.Format("Caching UP #{0}: Original {1} ..", op.OperationNumber, op.OriginalBounds);

                int howMany = op.Span / 2;
                if ((totCount - op.OriginalBounds.HighBound) < howMany)
                    howMany = (totCount - op.OriginalBounds.HighBound);

                if (howMany > 0)
                {
                    resultSetArray = _retrieveSourceElementsQueryFunction(op.OriginalBounds.HighBound, howMany, op.CancellationToken).ToArray();

                    logMessage = string.Format("{0} adding {1} records .. ", logMessage, howMany);

                    var resultingItemsSourceSize = Math.Min(2 * op.Span, resultSetArray.Length + (op.OriginalBounds.HighBound - op.OriginalBounds.LowBound));

                    newHBound = op.OriginalBounds.HighBound + howMany;
                    newLBound = newHBound - resultingItemsSourceSize;
                    newHWater = op.OriginalBounds.HighWater + howMany;
                    newLWater = op.OriginalBounds.LowWater + howMany;
                }
            }
            catch (Exception exc)
            {
                storedException = exc;
            }
            finally
            {
                if (!op.WasCancelled)
                {
                    if (storedException != null)
                    {
                        if (_logFunction != null)
                        {
                            _logFunction(6, string.Format("{0} --> Failed!", logMessage));
                        }
                        op.SetComplete(storedException);
                    }
                    else
                    {
                        IBounds newBounds = new Bounds(newLBound, newHBound, newLWater, newHWater);
                        if (_logFunction != null)
                        {
                            _logFunction(6, string.Format("{0} --> New {1}", logMessage, newBounds));
                        }
                        op.SetComplete(resultSetArray, newBounds, mustRefresh);
                    }
                }
            }
        }

        private void RepositionItemsSource(object boxedOperation)
        {
            Operation<T> op = boxedOperation as Operation<T>;
            Exception storedException = null;
            T[] newItemsArray = null;
            int newLBound = 0; int newHBound = 0;
            int newLWater = 0; int newHWater = 0;
            bool mustRefresh = false;

            try
            {
                int howMany = op.Span * 2;
                int totCount = _countSourceElementsFunction();

                if (op.WasCancelled)
                    return;

                if (totCount != op.OriginalTotalCount)
                {
                    newHBound = Math.Min(Span * 2, totCount);
                    newHWater = Math.Min(Span, newHBound);

                    if (_logFunction != null)
                    {
                        _logFunction(6, string.Format("Cache REPOSITIONING #{0}: Original {1} .. (TotalCount {2} != Op.OriginalCount {3})", op.OperationNumber, op.OriginalBounds, totCount, op.OriginalTotalCount));
                    }                    

                    newItemsArray = _retrieveSourceElementsQueryFunction(0, newHBound, op.CancellationToken).ToArray();
                    mustRefresh = true;
                    return;
                }

                if (op.WasCancelled)
                    return;

                if (_logFunction != null)
                {
                    _logFunction(6, string.Format("Cache REPOSITIONING #{0}: Original {1} .. ", op.OperationNumber, op.OriginalBounds));
                }

                int center = (op.Start - op.End) / 2 + op.Start;

                if (center - op.Span < 0)
                {
                    newLBound = 0;
                    if (newLBound + op.Span * 2 >= totCount)
                    {
                        newHBound = totCount;
                        newLWater = 0;
                        newHWater = totCount;
                    }
                    else
                    {
                        newHBound = newLBound + 2 * op.Span;
                        newLWater = newLBound + op.Span / 2;
                        newHWater = newHBound - op.Span / 2;
                    }
                }
                else if (center + op.Span >= totCount)
                {
                    newLBound = Math.Max(totCount - op.Span * 2, 0);
                    newHBound = totCount;
                    if (newHBound - newLBound > op.Span * 2)
                    {
                        newLWater = newLBound + op.Span / 2;
                        newHWater = newHWater - op.Span / 2;
                    }
                    else
                    {
                        newLWater = newLBound;
                        newHWater = newHBound;
                    }
                }
                else
                {
                    newLBound = center - op.Span;
                    newHBound = newLBound + op.Span * 2;
                    newLWater = newLBound + op.Span / 2;
                    newHWater = newHBound - op.Span / 2;
                }
                howMany = newHBound - newLBound;

                if (op.WasCancelled)
                    return;

                newItemsArray = _retrieveSourceElementsQueryFunction(newLBound, howMany, op.CancellationToken).ToArray();
                if (newItemsArray.Length != howMany)
                {
                    if (_logFunction != null)
                    {
                        _logFunction(1, string.Format("VirtualizedList records cache failed to reposition. Expected {0} records and instead received {1} (new low bound was {2})", howMany, newItemsArray.Length, newLBound));
                    }
                    throw new Exception(string.Format("VirtualizedList records cache failed to reposition. Expected {0} records and instead received {1} (new low bound was {2})", howMany, newItemsArray.Length, newLBound));
                }

                if (op.WasCancelled)
                    return;
            }
            catch (Exception exc)
            {
                storedException = exc;
            }
            finally
            {
                if (!op.WasCancelled)
                {
                    if (storedException != null)
                    {
                        op.SetComplete(storedException);                        
                    }
                    else
                    {
                        op.SetComplete(newItemsArray, new Bounds(newLBound, newHBound, newLWater, newHWater), mustRefresh);
                    }
                }
                else
                {
                    if (_logFunction != null)
                    {
                        _logFunction(6, string.Format("Cache REPOSITIONING #{0} - Cancelled.. Aborted! Not Firing Callback!", op.OperationNumber));
                    }
                }
            }
        }

        #endregion
    }
}
