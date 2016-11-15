namespace Rhythm.Caching.Core.Caches
{

    // Namespaces.
    using Comparers;
    using Enums;
    using System;
    using System.Collections.Generic;

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

        #region Instance Properties

        private DateTime? LastCache { get; set; }
        private Dictionary<string[], T> Instances { get; set; }
        private object InstanceLock { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InstanceCache()
        {
            InstanceLock = new object();
            LastCache = null;
            Instances = new Dictionary<string[], T>(new StringArrayComparer());
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
        public T Get(TimeSpan duration, Func<T> replenisher,
            CacheGetMethod method = CacheGetMethod.Default, params string[] keys)
        {

            // Which cache retrieval method should be used?
            if (method == CacheGetMethod.FromCache)
            {

                // Get directly from cache.
                lock (InstanceLock)
                {
                    return LastCache.HasValue
                        ? TryGetByKeys(keys)
                        : default(T);
                }

            }
            else if (method == CacheGetMethod.NoCache)
            {

                // Get directly from replenisher.
                return replenisher();

            }
            else
            {
                lock (InstanceLock)
                {

                    // Variables.
                    var now = DateTime.Now;

                    // Force a cache update?
                    if (method == CacheGetMethod.Recache)
                    {
                        UpdateValueByKeys(keys, replenisher(), false);
                    }

                    // Update cache if it's expired.
                    else if (!LastCache.HasValue || !Instances.ContainsKey(keys) || now.Subtract(LastCache.Value) >= duration)
                    {

                        // Get new value?
                        if (method == CacheGetMethod.NoStore)
                        {
                            return replenisher();
                        }

                        // Update cache with new value.
                        else
                        {
                            UpdateValueByKeys(keys, replenisher(), false);
                            LastCache = now;
                        }

                    }

                    // Return value.
                    return TryGetByKeys(keys);

                }
            }

        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            lock (InstanceLock)
            {
                LastCache = null;
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
        /// <returns>
        /// The value, or the default for the type.
        /// </returns>
        private T TryGetByKeys(string[] keys)
        {
            var chosenKeys = keys ?? EmptyArray;
            var value = default(T);
            lock (InstanceLock)
            {
                if (Instances.TryGetValue(chosenKeys, out value))
                {
                    return value;
                }
                else
                {
                    return default(T);
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
        /// <param name="doLock">
        /// Lock the instance cache during the update?
        /// </param>
        private void UpdateValueByKeys(string[] keys, T value, bool doLock = true)
        {
            if (doLock)
            {
                lock (InstanceLock)
                {
                    Instances[keys] = value;
                }
            }
            else
            {
                Instances[keys] = value;
            }
        }

        #endregion

    }

}