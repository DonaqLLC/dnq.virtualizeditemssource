using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DNQ.VirtualizedItemsSource
{
    /// <summary>   A base class for specialized weak caches based on a key-value pair paradigm where
    ///             values are stored as weak references. The cache will not prevent items from being
    ///             reclaimed by the Garbage Collector. </summary>
    ///
    /// <seealso cref="T:System.Collections.IEnumerable"/>
    public abstract class WeakCache
        : System.Collections.IEnumerable
        , IDisposable
    {
        private static List<WeakReference> _registeredCaches = new List<WeakReference>();

        static WeakCache()
        {
        }

        /// <summary>   Creates a new parameterized weak cache and sets its <see cref="AutoPurgeInterval"/> property. </summary>
        ///
        /// <typeparam name="TKeyType">     Type of the keys that will be used to reference items in the cache. </typeparam>
        /// <typeparam name="TItemType">    Type of the items that will be stored in the new cache. </typeparam>
        /// <param name="autoPurgeInterval"> The automatic purge interval for this cache. Set to
        ///     <see cref="TimeSpan.Zero"/> to disable automatic purging. </param>
        ///
        /// <returns>   . </returns>
        public static WeakCache<TKeyType, TItemType> Create<TKeyType, TItemType>(TimeSpan autoPurgeInterval) where TItemType : class
        {
            var newCache = new WeakCache<TKeyType, TItemType>(autoPurgeInterval);

            lock (_registeredCaches)
            {
                _registeredCaches.Add(new WeakReference(newCache));
            }

            if (autoPurgeInterval != TimeSpan.Zero)
            {
                AdjustAutoPurgeTimer();
            }

            return newCache;
        }

        /// <summary>  Creates a new parameterized weak cache with auto purging disabled. </summary>
        ///
        /// <typeparam name="TKeyType">     Type of the keys that will be used to reference items in the cache. </typeparam>
        /// <typeparam name="TItemType">    Type of the items that will be stored in the new cache. </typeparam>
        ///
        /// <returns>   . </returns>
        public static WeakCache<TKeyType, TItemType> Create<TKeyType, TItemType>() where TItemType : class
        {
            return Create<TKeyType, TItemType>(TimeSpan.Zero);
        }

        /// <summary>   Specialised constructor for use only by derived classes. It is invoked by the 
        ///             non-generic WeakCache object via the <see cref="Create"/> methods.</summary>
        ///
        /// <param name="autoPurgeInterval">    The automatic purge interval for this cache. 
        ///                                     Set to <see cref="TimeSpan.Zero"/> to disable automatic purging.</param>
        protected WeakCache(TimeSpan autoPurgeInterval)
        {
            AutoPurgeInterval = autoPurgeInterval;
        }

        /// <summary>   Gets the automatic purge interval for this weak cache (the time interval when
        ///             this cache is automatically purged of expired items). 
        ///             </summary>
        /// <remarks>   If set to Zero this cache is not automatically purged.</remarks>
        /// <value> The automatic purge interval for this cache. </value>
        public TimeSpan AutoPurgeInterval
        {
            get;
            private set;
        }

        /// <summary>   Clears the cache by removing all the items. </summary>
        public abstract void Clear();

        /// <summary>   Gets the number of elements in the cache (including potentially expired ones).  </summary>
        public abstract int Count { get; }

        /// <summary>   Gets the number of elements in the cache, excluding expired ones. This operation
        ///             is more expensive than a simple count.  </summary>
        public abstract int GetValidCount();

        /// <summary>   Purges the expired items from this cache. </summary>
        /// <returns>   A <see cref="PurgeResult"/> value indicating the number of items removed, or that an error occured. </returns>
        public abstract PurgeResult Purge();

        /// <summary>   Returns an enumerator that iterates through all items stored in the cache. </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through
        /// the collection of items in the cache, containing KeyValuePair elements.
        /// </returns>
        public System.Collections.IEnumerator GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        /// <summary>   Gets the internal enumerator. </summary>
        protected abstract System.Collections.IEnumerator InternalGetEnumerator();     

        /// <summary>   Forces a purge of all active weak caches and removes any references to caches
        ///             that are no longer references anywhere elese in memory. </summary>
        public static void PurgeAll()
        {
            bool registeredCachesChanged = false;

            lock (_registeredCaches)
            {
                for (int idx = _registeredCaches.Count - 1; idx >= 0; idx--)
                {
                    WeakReference wr = _registeredCaches[idx];
                    if (wr != null && wr.IsAlive && wr.Target != null)
                    {
                        ((WeakCache)wr.Target).Purge();
                    }
                    else
                    {
                        registeredCachesChanged = true;
                        _registeredCaches.RemoveAt(idx);
                    }
                }
            }

            if (registeredCachesChanged)
            {
                AdjustAutoPurgeTimer();
            }
        }

        /// <summary>
        /// Disposes the resources associated with this cache and removes the cache from the list of registered caches.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {            
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        bool registeredCachesChanged = false;

                        lock (_registeredCaches)
                        {
                            for (int idx = _registeredCaches.Count - 1; idx >= 0; idx--)
                            {
                                if (ReferenceEquals(_registeredCaches[idx].Target, this) || _registeredCaches[idx].IsAlive == false || _registeredCaches[idx].Target == null)
                                {
                                    _registeredCaches.RemoveAt(idx);
                                    registeredCachesChanged = true;
                                    break;
                                }
                            }
                        }

                        if (registeredCachesChanged)
                        {
                            AdjustAutoPurgeTimer();
                        }
                    }
                    catch
                    {
                        // do nothing if an exception occurs here.. we're not supposed to throw from Dispose
                    }
                }

                _disposed = true;
            }
        }

        #region Auto Purging Functionality

        private static int ComputeGCD(int a, int b)
        {
            int c;
            while (a != 0)
            {
                c = a;
                a = b % a;
                b = c;
            }
            return b;
        }

        private static int ComputeTimerFrequency()
        {
            lock (_registeredCaches)
            {
                if (_registeredCaches.Count == 0)
                    return 0;

                int lastgcd = 0;

                for (int idx = _registeredCaches.Count - 1; idx >= 0; idx--)
                {
                    WeakReference wr = _registeredCaches[idx];
                    if (wr != null && wr.IsAlive && wr.Target != null)
                    {
                        WeakCache cache = (WeakCache)wr.Target;
                        TimeSpan interval = cache.AutoPurgeInterval;

                        if (interval != TimeSpan.Zero)
                        {
                            int minutes = (int)Math.Ceiling(interval.TotalMinutes);
                            if (lastgcd == 0)
                                lastgcd = minutes;
                            else if (minutes != 0)
                            {
                                lastgcd = ComputeGCD(minutes, lastgcd);
                            }
                        }
                    }
                    else
                    {
                        _registeredCaches.RemoveAt(idx);
                    }
                }

                return lastgcd;
            }
        }

        private static int _lastTimerFrequency = 0;
        private static Timer _autoPurgeTimer;

        public static void AdjustAutoPurgeTimer()
        {
            int timerFrequency = ComputeTimerFrequency();
            if (timerFrequency != _lastTimerFrequency)
            {
                if (_lastTimerFrequency == 0)       // the timer has just been started
                {
                    _autoPurgeTimer = new Timer(AutoPurgeCallback, null, timerFrequency * 60000, timerFrequency * 60000);
                }
                else if(timerFrequency == 0)        // the timer has just been stopped
                {
                    _autoPurgeTimer.Dispose();
                }
                else
                {
                    _autoPurgeTimer.Change(timerFrequency * 60000, timerFrequency * 60000);
                }
                
                _lastTimerFrequency = timerFrequency;                
            }
        }

        public static void AutoPurgeCallback(object state)
        {
            bool registeredCachesChanged = false;

            lock (_registeredCaches)
            {
                for (int idx = _registeredCaches.Count - 1; idx >= 0; idx--)
                {
                    WeakReference wr = _registeredCaches[idx];
                    if (wr != null && wr.IsAlive && wr.Target != null)
                    {
                        WeakCache cache = (WeakCache)wr.Target;
                        if (cache.AutoPurgeInterval != TimeSpan.Zero)
                        {
                            cache.Purge();
                        }
                    }
                    else
                    {
                        registeredCachesChanged = true;
                        _registeredCaches.RemoveAt(idx);
                    }
                }
            }

            if (registeredCachesChanged)
            {
                AdjustAutoPurgeTimer();
            }
        }

        #endregion       
    }
}
