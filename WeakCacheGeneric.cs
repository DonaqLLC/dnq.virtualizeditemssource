using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DNQ.VirtualizedItemsSource
{
    /// <summary>   Implements a specialized weak cache system based on a key-value pair paradigm where
    ///             values are stored as weak references. The cache will not prevent items from being
    ///             reclaimed by the Garbage Collector. </summary>
    ///
    /// <typeparam name="KeyType">      Type of the keys used to reference items in the cache. </typeparam>
    /// <typeparam name="TItemType">    Type of the items stored in the cache. </typeparam>
    ///
    /// <seealso cref="T:DNQ.VirtualizedItemsSource.WeakCache"/>
    /// <seealso cref="T:System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{KeyType,TItemType}}"/>
    public class WeakCache<KeyType, TItemType>
        : WeakCache
        , IEnumerable<KeyValuePair<KeyType, TItemType>> where TItemType : class
    {
        private Dictionary<KeyType, WeakReference> cache = new Dictionary<KeyType, WeakReference>();

        private ReaderWriterLockSlim _internalLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        internal WeakCache(TimeSpan autoPurgeInterval)
            : base(autoPurgeInterval)
        {
        }

        /// <summary>   Clears the cache by removing all the items. </summary>
        public override void Clear()
        {
            _internalLock.EnterWriteLock();
            try
            {                
                cache.Clear();
            }
            finally
            {
                _internalLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the number of elements in the cache (including potentially expired ones).
        /// </summary>
        ///
        /// <value> The number of items in the cache (including potentially expired ones). </value>
        public override int Count
        {
            get
            {                
                _internalLock.EnterReadLock();
                try
                {
                    return cache.Count;
                }
                finally
                {
                    _internalLock.ExitReadLock();
                }
            }
        }

        /// <summary>   The purge timeout internal. May be used only from this class, as paramter 
        ///             to TryEnterWriteLock(TimeSpan timoutInterval) methods. All others muse use
        ///             the <see cref="PurgeTimeout"/> member.</summary>
        private TimeSpan _purgeTimeout_INTERNAL_UNSAFE = TimeSpan.FromMilliseconds(500);
        public TimeSpan PurgeTimeout
        {
            get
            {
                _internalLock.EnterReadLock();
                try
                {
                    return _purgeTimeout_INTERNAL_UNSAFE;
                }
                finally
                {
                    _internalLock.ExitReadLock();
                }
            }
            set
            {
                _internalLock.EnterWriteLock();
                try
                {
                    _purgeTimeout_INTERNAL_UNSAFE = value;
                }
                finally
                {
                    _internalLock.ExitWriteLock();
                }
            }
        }

        /// <summary>   Gets the number of elements in the cache, excluding expired ones. This operation
        ///             is more expensive than a simple count.  </summary>
        public override int GetValidCount()
        {
            _internalLock.EnterReadLock();
            try
            {
                int count = 0;
                foreach (var kvp in cache)
                {
                    if (kvp.Value != null && kvp.Value.IsAlive && kvp.Value.Target != null)
                        count++;
                }
                return count;
            }
            finally
            {
                _internalLock.ExitReadLock();
            }
        }

        /// <summary>   Gets the item references by the given, if the item is in the cache and is not expired. </summary>
        ///
        /// <param name="key">  A key of an item to be looked up in the cache. </param>
        ///
        /// <returns>   The item if it exists and is not expired, otherwise null. </returns>
        public TItemType Get(KeyType key)
        {
            _internalLock.EnterUpgradeableReadLock();
            try
            {
                if (cache.ContainsKey(key))
                {
                    if (cache[key].IsAlive && cache[key].Target != null)
                    {
                        return (TItemType)cache[key].Target;
                    }
                    else
                    {
                        if (_internalLock.TryEnterWriteLock(_purgeTimeout_INTERNAL_UNSAFE))
                        {
                            try
                            {
                                cache.Remove(key);
                            }
                            finally
                            {
                                _internalLock.ExitWriteLock();
                            }
                        }
                        // if we couldn't enter the write lock within the designated timout interval, we can ignore it here.. 
                        // another chance will present itself
                    }
                }
            
                return null;
            }
            finally
            {
                _internalLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>   Tests whether a non-expired item with the given key exists in the cache.</summary>
        ///
        /// <param name="key">  A key of an item to be looked up in the cache. </param>
        ///
        /// <returns>   True if the item exists and is not expired, false otherwise. </returns>
        public bool Has(KeyType key)
        {
            _internalLock.EnterReadLock();
            try
            {
                return (cache.ContainsKey(key) && cache[key].IsAlive && cache[key].Target != null);
            }
            finally
            {
                _internalLock.ExitReadLock();
            }
        }

        /// <summary>   Puts an items in the cache. If the item already exists it will be overwritten. </summary>
        ///
        /// <param name="key">  A key of an item to be looked up in the cache. </param>
        /// <param name="item"> The item to be added to the cache. </param>
        public void Put(KeyType key, TItemType item)
        {
            _internalLock.EnterWriteLock();
            try
            {                
                cache[key] = new WeakReference(item);
            }
            finally
            {
                _internalLock.ExitWriteLock();
            }
        }

        public TItemType this[KeyType key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Put(key, value);
            }
        }

        /// <summary>   Purges the expired items from this cache. </summary>
        public override PurgeResult Purge()
        {
            _internalLock.EnterUpgradeableReadLock();
            try
            {
                List<KeyType> keysToRemove = new List<KeyType>();
                foreach (KeyType key in cache.Keys)
                {
                    if (cache[key] == null || !cache[key].IsAlive || cache[key].Target == null)
                        keysToRemove.Add(key);
                }

                if (keysToRemove.Count > 0)
                {
                    if (_internalLock.TryEnterWriteLock(_purgeTimeout_INTERNAL_UNSAFE))
                    {
                        try
                        {
                            foreach (KeyType key in keysToRemove)
                            {
                                cache.Remove(key);
                            }

                            return new PurgeResult(keysToRemove.Count);
                        }
                        finally
                        {
                            _internalLock.ExitWriteLock();
                        }                        
                    }
                    else
                    {
                        return PurgeResult.ErrorTimedout;
                    }
                }
                else
                {
                    return PurgeResult.NotNeeded;
                }
            }
            finally
            {
                _internalLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>   Gets an enumerator for all non-expired items and their keys and
        ///             performs a Purge sweep as it enumerates the items. </summary>
        /// <returns>   The enumerator which allows foreach to be used to enumerate all 
        ///             non-expired items in the cache along with their keys </returns>
        public IEnumerator<KeyValuePair<KeyType, TItemType>> GetEnumerator()
        {
            List<KeyValuePair<KeyType, TItemType>> elements = new List<KeyValuePair<KeyType, TItemType>>();
             
            _internalLock.EnterUpgradeableReadLock();
            try
            {
                List<KeyType> keysToRemove = new List<KeyType>();
                foreach (var kvp in cache)
                {
                    if (kvp.Value != null && kvp.Value.IsAlive && kvp.Value.Target != null)
                    {
                        elements.Add(new KeyValuePair<KeyType, TItemType>(kvp.Key, (TItemType)kvp.Value.Target));
                    }
                    else
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                if (keysToRemove.Count > 0)
                {
                    if (_internalLock.TryEnterWriteLock(_purgeTimeout_INTERNAL_UNSAFE))
                    {
                        try
                        {
                            foreach (KeyType key in keysToRemove)
                            {
                                cache.Remove(key);
                            }
                        }
                        finally
                        {
                            _internalLock.ExitWriteLock();
                        }
                    }
                    else
                    {
                        // We couldn't do the purge because we were blocked!
                    }
                }
            }
            finally
            {
                _internalLock.ExitUpgradeableReadLock();
            }

            return elements.GetEnumerator();
        }

        /// <summary>   Gets the internal enumerator, used to provide enumeration capabilities to the base class. </summary>
        protected override System.Collections.IEnumerator InternalGetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>   Enumerates the items in the cache and performs a Purge sweep. </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to enumerate the non-expired items in the cache.
        /// </returns>
        public IEnumerable<TItemType> EnumerateValues()
        {
            List<KeyType> keysToRemove = new List<KeyType>();
            List<TItemType> items = new List<TItemType>();
            
            _internalLock.EnterUpgradeableReadLock();
            try
            {
                foreach (var kvp in cache)
                {
                    if (kvp.Value != null && kvp.Value.IsAlive && kvp.Value.Target != null)
                    {
                        items.Add((TItemType)kvp.Value.Target);
                    }
                    else
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                if (keysToRemove.Count > 0)
                 {
                    if (_internalLock.TryEnterWriteLock(_purgeTimeout_INTERNAL_UNSAFE))
                    {
                        try
                        {
                             foreach (KeyType key in keysToRemove)
                             {
                                 cache.Remove(key);
                             }
                        }
                        finally
                        {
                            _internalLock.ExitWriteLock();
                        }
                    }
                    else
                    {
                        // We couldn't do the purge because we were blocked!
                    }
                 }
            }
            finally
            {
                _internalLock.ExitUpgradeableReadLock();
            }

            return (IEnumerable<TItemType>)items;
        }

        /// <summary>   Enumerates the keys in the cache and performs a Purge sweep. </summary>
        ///
        /// <returns>
        /// An enumerator that allows foreach to be used to enumerate the keys to non-expired items in the cache.
        /// </returns>
        public IEnumerable<KeyType> EnumerateKeys()
        {
            List<KeyType> keysToRemove = new List<KeyType>();
            List<KeyType> keys = new List<KeyType>();

            _internalLock.EnterUpgradeableReadLock();
            try
            {
                foreach (var kvp in cache)
                {
                    if (kvp.Value != null && kvp.Value.IsAlive && kvp.Value.Target != null)
                    {
                        keys.Add(kvp.Key);
                    }
                    else
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                if (keysToRemove.Count > 0)
                {
                    if (_internalLock.TryEnterWriteLock(_purgeTimeout_INTERNAL_UNSAFE))
                    {
                        try
                        {
                            foreach (KeyType key in keysToRemove)
                            {
                                cache.Remove(key);
                            }
                        }
                        finally
                        {
                            _internalLock.ExitWriteLock();
                        }
                    }
                    else
                    {
                        // We couldn't do the purge because we were blocked!
                    }
                }
            }
            finally
            {
                _internalLock.ExitUpgradeableReadLock();
            }

            return (IEnumerable<KeyType>)keys;
        }
    }
}
