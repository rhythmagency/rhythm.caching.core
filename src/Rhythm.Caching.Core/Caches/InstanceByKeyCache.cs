﻿namespace Rhythm.Caching.Core.Caches
{

    // Namespaces.
    using Comparers;
    using Enums;
    using System;
    using System.Collections.Generic;

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

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InstanceByKeyCache()
        {
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

            // Which cache retrieval method should be used?
            if (method == CacheGetMethod.FromCache)
            {

                // Get directly from cache.
                lock (InstancesLock)
                {
                    return TryGetByKeys(keys, key);
                }

            }
            else if (method == CacheGetMethod.NoCache)
            {

                // Get directly from replenisher.
                return replenisher(key);

            }
            else
            {
                lock (InstancesLock)
                {
                    var tempInstance = default(T);
                    var now = DateTime.Now;
                    if (method == CacheGetMethod.Recache)
                    {

                        // Force the cache to replenish.
                        tempInstance = replenisher(key);
                        UpdateValueByKeys(keys, key, tempInstance, now, false);

                    }
                    else
                    {

                        // Value already cached?
                        var tempTuple = default(Tuple<Dictionary<string[], T>, DateTime>);
                        if (Instances.TryGetValue(key, out tempTuple)
                            && tempTuple.Item1.ContainsKey(keys))
                        {
                            if (now.Subtract(Instances[key].Item2) >= duration)
                            {
                                if (method == CacheGetMethod.NoStore)
                                {

                                    // Cache expired. Get a new value without modifying the cache.
                                    tempInstance = replenisher(key);

                                }
                                else
                                {

                                    // Cache expired. Replenish the cache.
                                    tempInstance = replenisher(key);
                                    UpdateValueByKeys(keys, key, tempInstance, now, false);

                                }
                            }
                            else
                            {

                                // Cache still valid. Use cached value.
                                tempInstance = TryGetByKeys(keys, key);

                            }
                        }
                        else
                        {
                            if (method == CacheGetMethod.NoStore)
                            {

                                // No cached value. Get a new value without modifying the cache.
                                tempInstance = replenisher(key);

                            }
                            else
                            {

                                // No cached value. Replenish the cache.
                                tempInstance = replenisher(key);
                                UpdateValueByKeys(keys, key, tempInstance, now, false);

                            }
                        }

                    }
                    return tempInstance;
                }
            }

        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            lock (InstancesLock)
            {
                Instances.Clear();
            }
        }

        /// <summary>
        /// Clears the cache of the specified keys.
        /// </summary>
        /// <param name="keys">The keys to clear the cache of.</param>
        public void ClearKeys(IEnumerable<TKey> keys)
        {
            lock (InstancesLock)
            {
                foreach (var key in keys)
                {
                    Instances.Remove(key);
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
        /// <returns>
        /// The value, or the default for the type.
        /// </returns>
        private T TryGetByKeys(string[] keys, TKey accessKey)
        {
            var chosenKeys = keys ?? EmptyArray;
            var value = default(T);
            lock (InstancesLock)
            {
                var valueDictionary = default(Tuple<Dictionary<string[], T>, DateTime>);
                if (Instances.TryGetValue(accessKey, out valueDictionary))
                {
                    if (valueDictionary.Item1.TryGetValue(chosenKeys, out value))
                    {
                        return value;
                    }
                }
            }
            return default(T);
        }

        /// <summary>
        /// Updates the cache value by the specified keys.
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
        /// <param name="doLock">
        /// Lock the instance cache during the update?
        /// </param>
        private void UpdateValueByKeys(string[] keys, TKey accessKey, T value,
            DateTime lastCache, bool doLock = true)
        {
            if (doLock)
            {
                lock (InstancesLock)
                {
                    UpdateValueByKeysWithoutLock(keys, accessKey, value, lastCache);
                }
            }
            else
            {
                UpdateValueByKeysWithoutLock(keys, accessKey, value, lastCache);
            }
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