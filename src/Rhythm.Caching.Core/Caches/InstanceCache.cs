namespace Rhythm.Caching.Core.Caches
{

    // Namespaces.
    using Comparers;
    using Enums;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Caches an instance variable.
    /// </summary>
    /// <typeparam name="T">
    /// The type of variable to cache.
    /// </typeparam>
    public class InstanceCache<T>
    {

        #region Static Variables

        private static string[] EmptyArray = new string[] { };

        #endregion

        #region Properties

        private DateTime? LastCache { get; set; }
        private Dictionary<string[], T> Instances { get; set; }
        private object InstanceLock { get; set; }
        private TimeSpan LockTimeout { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InstanceCache() : this(CacheSettings.DefaultLockTimeout)
        {
        }

        /// <summary>
        /// Constructor to specify a lock timeout.
        /// </summary>
        /// <param name="timeout">
        /// The amount of time to wait before giving up on locking.
        /// </param>
        public InstanceCache(TimeSpan timeout)
        {
            LockTimeout = timeout;
            InstanceLock = new object();
            LastCache = null;
            Instances = new Dictionary<string[], T>(new ArrayComparer<string>());
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the instance variable (either from the cache or from the specified function).
        /// </summary>
        /// <param name="duration">
        /// The duration to cache for.
        /// </param>
        /// <param name="replenisher">
        /// The function that replenishes the cache.
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
        public T Get(TimeSpan duration, Func<T> replenisher, CacheGetMethod method = CacheGetMethod.Default,
            params string[] keys)
        {
            var gotValue = default(bool);
            return TryGet(duration, replenisher, default(T), out gotValue, method,
                keys);
        }

        /// <summary>
        /// Gets the instance variable (either from the cache or from the specified function).
        /// </summary>
        /// <param name="duration">
        /// The duration to cache for.
        /// </param>
        /// <param name="replenisher">
        /// The function that replenishes the cache.
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
        public T TryGet(TimeSpan duration, Func<T> replenisher, T defaultValue, out bool gotValue,
            CacheGetMethod method = CacheGetMethod.Default, params string[] keys)
        {

            // Which cache retrieval method should be used?
            if (method == CacheGetMethod.FromCache)
            {

                // Get directly from cache.
                var lockTaken = default(bool);
                try
                {
                    Monitor.TryEnter(InstanceLock, LockTimeout, ref lockTaken);
                    if (lockTaken)
                    {
                        if (LastCache.HasValue)
                        {
                            var returnValue = TryGetByKeys(keys, out gotValue);
                            return returnValue;
                        }
                        else
                        {
                            gotValue = false;
                            return defaultValue;
                        }
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
                        Monitor.Exit(InstanceLock);
                    }
                }

            }
            else if (method == CacheGetMethod.NoCache)
            {

                // Get directly from replenisher.
                var returnValue = replenisher();
                gotValue = true;
                return returnValue;

            }
            else
            {
                var returnValue = defaultValue;
                var lockTaken = default(bool);
                try
                {
                    Monitor.TryEnter(InstanceLock, LockTimeout, ref lockTaken);
                    if (lockTaken)
                    {

                        // Variables.
                        var now = DateTime.Now;

                        // Force a cache update?
                        if (method == CacheGetMethod.Recache)
                        {
                            UpdateValueByKeys(keys, replenisher());
                        }

                        // Update cache if it's expired.
                        else if (!LastCache.HasValue || !Instances.ContainsKey(keys)
                            || now.Subtract(LastCache.Value) >= duration)
                        {

                            // Get new value?
                            if (method == CacheGetMethod.NoStore)
                            {
                                returnValue = replenisher();
                                gotValue = true;
                                return returnValue;
                            }

                            // Update cache with new value.
                            else
                            {
                                UpdateValueByKeys(keys, replenisher());
                                LastCache = now;
                            }

                        }

                        // Return value.
                        returnValue = TryGetByKeys(keys, out gotValue);
                        return returnValue;

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
                        Monitor.Exit(InstanceLock);
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
                Monitor.TryEnter(InstanceLock, LockTimeout, ref lockTaken);
                if (lockTaken)
                {
                    cleared = false;
                }
                else
                {
                    CacheSettings.FailedLockHandler(LockTimeout);
                    LastCache = null;
                    cleared = true;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(InstanceLock);
                }
            }
        }

        /// <summary>
        /// Indicates whether or not an instance was cached by the specified keys.
        /// </summary>
        /// <param name="keys">
        /// Optional. The keys to store/retrieve a value by. Each key combination will
        /// be treated as a separate cache.
        /// </param>
        /// <returns>
        /// True, if an item was cached by the specified keys; otherwise, false.
        /// </returns>
        public bool WasCached(params string[] keys)
        {
            var found = default(bool);
            return WasCached(out found, keys);
        }

        /// <summary>
        /// Indicates whether or not an instance was cached by the specified keys.
        /// </summary>
        /// <param name="successful">
        /// Was the check successful (e.g., wouldn't be successful if the lock
        /// timeout expired).
        /// </param>
        /// <param name="keys">
        /// Optional. The keys to store/retrieve a value by. Each key combination will
        /// be treated as a separate cache.
        /// </param>
        /// <returns>
        /// True, if an item was cached by the specified keys; otherwise, false.
        /// </returns>
        public bool WasCached(out bool successful, params string[] keys)
        {
            var lockTaken = default(bool);
            try
            {
                Monitor.TryEnter(InstanceLock, LockTimeout, ref lockTaken);
                if (lockTaken)
                {
                    successful = true;
                    return LastCache.HasValue
                        ? Instances.ContainsKey(keys)
                        : false;
                }
                else
                {
                    CacheSettings.FailedLockHandler(LockTimeout);
                    successful = false;
                    return false;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(InstanceLock);
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
        /// <param name="gotValue">
        /// True, if an existing value was retrieved; otherwise, false.
        /// </param>
        /// <returns>
        /// The value, or the default for the type.
        /// </returns>
        private T TryGetByKeys(string[] keys, out bool gotValue)
        {
            var chosenKeys = keys ?? EmptyArray;
            var value = default(T);
            var lockTaken = default(bool);
            try
            {
                Monitor.TryEnter(InstanceLock, LockTimeout, ref lockTaken);
                if (lockTaken)
                {
                    if (Instances.TryGetValue(chosenKeys, out value))
                    {
                        gotValue = true;
                        return value;
                    }
                    else
                    {
                        gotValue = false;
                        return default(T);
                    }
                }
                else
                {
                    CacheSettings.FailedLockHandler(LockTimeout);
                    gotValue = false;
                    return default(T);
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(InstanceLock);
                }
            }
        }

        /// <summary>
        /// Updates the cache value by the specified keys.
        /// </summary>
        /// <param name="keys">
        /// The keys to cache by.
        /// </param>
        /// <param name="value">
        /// The value to update the cache with.
        /// </param>
        private void UpdateValueByKeys(string[] keys, T value)
        {
            Instances[keys] = value;
        }

        #endregion

    }

}