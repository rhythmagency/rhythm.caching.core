namespace Rhythm.Caching.Core.Caches
{

    // Namespaces.
    using Comparers;
    using Enums;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Caches instance variables by key in a dictionary-like structure.
    /// </summary>
    /// <typeparam name="T">
    /// The type of value to cache.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// The key to use when access values in the dictionary.
    /// </typeparam>
    public class InstanceByKeyCache<T, TKey>
    {

        #region Static Variables

        private static string[] EmptyArray = new string[] { };

        #endregion

        #region Properties

        /// <summary>
        /// The instances stored by their key, then again by a contextual key.
        /// </summary>
        private Dictionary<TKey, Tuple<Dictionary<string[], T>, DateTime>> Instances { get; set; }

        /// <summary>
        /// Object to perform locks for cross-thread safety.
        /// </summary>
        private object InstancesLock { get; set; }

        /// <summary>
        /// The amount of time to wait until giving up on a lock.
        /// </summary>
        private TimeSpan LockTimeout { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InstanceByKeyCache() : this(CacheSettings.DefaultLockTimeout)
        {
        }

        /// <summary>
        /// Constructor to specify a lock timeout.
        /// </summary>
        /// <param name="timeout">
        /// The amount of time to wait before giving up on locking.
        /// </param>
        public InstanceByKeyCache(TimeSpan timeout)
        {
            LockTimeout = timeout;
            InstancesLock = new object();
            Instances = new Dictionary<TKey, Tuple<Dictionary<string[], T>, DateTime>>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the instance variable (either from the cache or from the specified function).
        /// </summary>
        /// <param name="key">
        /// The key to use when fetching the variable.
        /// </param>
        /// <param name="replenisher">
        /// The function that replenishes the cache.
        /// </param>
        /// <param name="duration">
        /// The duration to cache for.
        /// </param>
        /// <param name="method">
        /// Optional. The cache method to use when retrieving the value.
        /// </param>
        /// <param name="keys">
        /// Optional. The keys to store/retrieve a value by. Each key combination will
        /// be treated as a separate cache.
        /// </param>
        /// <returns>
        /// The value.
        /// </returns>
        public T Get(TKey key, Func<TKey, T> replenisher, TimeSpan duration,
            CacheGetMethod method = CacheGetMethod.Default, params string[] keys)
        {
            var gotValue = default(bool);
            return TryGet(key, replenisher, duration, default(T), out gotValue, method, keys);
        }

        /// <summary>
        /// Gets the instance variable (either from the cache or from the specified function).
        /// </summary>
        /// <param name="key">
        /// The key to use when fetching the variable.
        /// </param>
        /// <param name="replenisher">
        /// The function that replenishes the cache.
        /// </param>
        /// <param name="duration">
        /// The duration to cache for.
        /// </param>
        /// <param name="defaultValue">
        /// The value to use in the event that a value could not be retrieved (e.g.,
        /// if the lock could not be achieved within the timeout).
        /// </param>
        /// <param name="gotValue">
        /// Was the value retrieved, or was the default value used instead?
        /// </param>
        /// <param name="method">
        /// Optional. The cache method to use when retrieving the value.
        /// </param>
        /// <param name="keys">
        /// Optional. The keys to store/retrieve a value by. Each key combination will
        /// be treated as a separate cache.
        /// </param>
        /// <returns>
        /// The value.
        /// </returns>
        public T TryGet(TKey key, Func<TKey, T> replenisher, TimeSpan duration, T defaultValue,
            out bool gotValue, CacheGetMethod method = CacheGetMethod.Default, params string[] keys)
        {

            // Which cache retrieval method should be used?
            if (method == CacheGetMethod.FromCache)
            {

                // Get directly from cache.
                var lockTaken = default(bool);
                try
                {
                    Monitor.TryEnter(InstancesLock, LockTimeout, ref lockTaken);
                    if (lockTaken)
                    {
                        return TryGetByKeys(keys, key, out gotValue);
                    }
                    else
                    {
                        CacheSettings.FailedLockHandler(LockTimeout);
                        gotValue = false;
                        return defaultValue;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(InstancesLock);
                    }
                }

            }
            else if (method == CacheGetMethod.NoCache)
            {

                // Get directly from replenisher.
                var returnValue = replenisher(key);
                gotValue = true;
                return returnValue;

            }
            else
            {
                var lockTaken = default(bool);
                try
                {
                    Monitor.TryEnter(InstancesLock, LockTimeout, ref lockTaken);
                    if (lockTaken)
                    {
                        var tempInstance = default(T);
                        var now = DateTime.Now;
                        if (method == CacheGetMethod.Recache)
                        {

                            // Force the cache to replenish.
                            tempInstance = replenisher(key);
                            gotValue = true;
                            UpdateValueByKeysWithoutLock(keys, key, tempInstance, now);

                        }
                        else
                        {

                            // Value already cached?
                            var tempTuple = default(Tuple<Dictionary<string[], T>, DateTime>);
                            if (Instances.TryGetValue(key, out tempTuple) && tempTuple.Item1.ContainsKey(keys))
                            {
                                if (now.Subtract(Instances[key].Item2) >= duration)
                                {
                                    if (method == CacheGetMethod.NoStore)
                                    {

                                        // Cache expired. Get a new value without modifying the cache.
                                        tempInstance = replenisher(key);
                                        gotValue = true;

                                    }
                                    else
                                    {

                                        // Cache expired. Replenish the cache.
                                        tempInstance = replenisher(key);
                                        gotValue = true;
                                        UpdateValueByKeysWithoutLock(keys, key, tempInstance, now);

                                    }
                                }
                                else
                                {

                                    // Cache still valid. Use cached value.
                                    tempInstance = TryGetByKeys(keys, key, out gotValue);

                                }
                            }
                            else
                            {
                                if (method == CacheGetMethod.NoStore)
                                {

                                    // No cached value. Get a new value without modifying the cache.
                                    tempInstance = replenisher(key);
                                    gotValue = true;

                                }
                                else
                                {

                                    // No cached value. Replenish the cache.
                                    tempInstance = replenisher(key);
                                    gotValue = true;
                                    UpdateValueByKeysWithoutLock(keys, key, tempInstance, now);

                                }
                            }

                        }
                        return tempInstance;
                    }
                    else
                    {
                        CacheSettings.FailedLockHandler(LockTimeout);
                        gotValue = false;
                        return defaultValue;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(InstancesLock);
                    }
                }
            }

        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            var cleared = default(bool);
            Clear(out cleared);
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        /// <param name="cleared">
        /// Was the cache cleared successfully?
        /// </param>
        public void Clear(out bool cleared)
        {
            var lockTaken = default(bool);
            try
            {
                Monitor.TryEnter(InstancesLock, LockTimeout, ref lockTaken);
                if (lockTaken)
                {
                    cleared = false;
                }
                else
                {
                    CacheSettings.FailedLockHandler(LockTimeout);
                    Instances.Clear();
                    cleared = true;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(InstancesLock);
                }
            }
        }

        /// <summary>
        /// Clears the cache of the specified keys.
        /// </summary>
        /// <param name="keys">The keys to clear the cache of.</param>
        public void ClearKeys(IEnumerable<TKey> keys)
        {
            var cleared = default(bool);
            ClearKeys(keys, out cleared);
        }

        /// <summary>
        /// Clears the cache of the specified keys.
        /// </summary>
        /// <param name="keys">The keys to clear the cache of.</param>
        /// <param name="cleared">
        /// Were the keys cleared successfully?
        /// </param>
        public void ClearKeys(IEnumerable<TKey> keys, out bool cleared)
        {
            var lockTaken = default(bool);
            try
            {
                Monitor.TryEnter(InstancesLock, LockTimeout, ref lockTaken);
                if (lockTaken)
                {
                    foreach (var key in keys)
                    {
                        Instances.Remove(key);
                    }
                    cleared = true;
                }
                else
                {
                    CacheSettings.FailedLockHandler(LockTimeout);
                    cleared = false;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(InstancesLock);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Trys to get the value by the specified keys.
        /// </summary>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <param name="accessKey">
        /// The key to use to access the value.
        /// </param>
        /// <param name="gotValue">
        /// True, if an existing value was retrieved; otherwise, false.
        /// </param>
        /// <returns>
        /// The value, or the default for the type.
        /// </returns>
        private T TryGetByKeys(string[] keys, TKey accessKey, out bool gotValue)
        {
            var chosenKeys = keys ?? EmptyArray;
            var value = default(T);
            var lockTaken = default(bool);
            try
            {
                Monitor.TryEnter(InstancesLock, LockTimeout, ref lockTaken);
                if (lockTaken)
                {
                    var valueDictionary = default(Tuple<Dictionary<string[], T>, DateTime>);
                    if (Instances.TryGetValue(accessKey, out valueDictionary))
                    {
                        if (valueDictionary.Item1.TryGetValue(chosenKeys, out value))
                        {
                            gotValue = true;
                            return value;
                        }
                    }
                }
                else
                {
                    CacheSettings.FailedLockHandler(LockTimeout);
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(InstancesLock);
                }
            }
            gotValue = false;
            return default(T);
        }

        /// <summary>
        /// Updates the cache with the specified value.
        /// </summary>
        /// <param name="keys">
        /// The keys to cache by.
        /// </param>
        /// <param name="accessKey">
        /// The key to use to access the value.
        /// </param>
        /// <param name="value">
        /// The value to update the cache with.
        /// </param>
        /// <param name="lastCache">
        /// The date/time to mark the cache as last updated.
        /// </param>
        private void UpdateValueByKeysWithoutLock(string[] keys, TKey accessKey,
            T value, DateTime lastCache)
        {

            // Variables.
            var instanceTuple = default(Tuple<Dictionary<string[], T>, DateTime>);
            var instanceDictionary = default(Dictionary<string[], T>);

            // Get or create the dictionary.
            if (Instances.TryGetValue(accessKey, out instanceTuple))
            {
                instanceDictionary = instanceTuple.Item1;
            }
            else
            {
                instanceDictionary = new Dictionary<string[], T>(new StringArrayComparer());
            }

            // Update the value in the dictionary.
            instanceDictionary[keys] = value;

            // Update the last cache date.
            Instances[accessKey] = new Tuple<Dictionary<string[], T>, DateTime>(
                instanceDictionary, lastCache);

        }

        #endregion

    }

}