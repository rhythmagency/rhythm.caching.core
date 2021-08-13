namespace Rhythm.Caching.Core.Tests
{

    // Namespaces.
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rhythm.Caching.Core.Caches;
    using Rhythm.Caching.Core.KeyTypes;
    using System;

    /// <summary>
    /// Tests for InstanceByKeyCache.
    /// </summary>
    [TestClass]
    public class InstanceByKeyCacheTest
    {

        #region Properties

        private TimeSpan CacheDuration => TimeSpan.FromHours(1);

        #endregion

        #region Test Methods

        /// <summary>
        /// Test to ensure you get the correct value from the cache.
        /// </summary>
        [TestMethod]
        public void GetValueTest()
        {
            var cache = new InstanceByKeyCache<string, string>();
            var key = "hi";
            var value = cache.Get(key, (localKey) =>
            {
                return "Hello";
            }, CacheDuration);
            Assert.AreEqual("Hello", value);
        }

        /// <summary>
        /// Test to validate that caching cannot be done using an array as the key.
        /// </summary>
        [TestMethod]
        public void UnableToCacheByArrayKeyTest()
        {
            var cache = new InstanceByKeyCache<string, int[]>();
            var key1 = new[] { 1, 2, 3 };
            var key2 = new[] { 1, 2, 3 };
            var value = cache.Get(key1, (localKey) =>
            {
                return "First";
            }, CacheDuration);
            value = cache.Get(key2, (localKey) =>
            {
                return "Second";
            }, CacheDuration);
            Assert.AreEqual("Second", value);
        }

        /// <summary>
        /// Test to validate that caching can be done with an <see cref="ArrayKey{T}"/>.
        /// </summary>
        [TestMethod]
        public void CacheByArrayKeyTest()
        {
            var cache = new InstanceByKeyCache<string, ArrayKey<int>>();
            var key1 = new ArrayKey<int>(new[] { 1, 2, 3 });
            var key2 = new ArrayKey<int>(new[] { 1, 2, 3 });
            var value = cache.Get(key1, (localKey) =>
            {
                return "First";
            }, CacheDuration);
            value = cache.Get(key2, (localKey) =>
            {
                return "Second";
            }, CacheDuration);
            Assert.AreEqual("First", value);
        }

        /// <summary>
        /// Test to validate that caching by two different <see cref="ArrayKey{T}"/> instances
        /// will result in two different values.
        /// </summary>
        [TestMethod]
        public void CacheByIncorrectArrayKeyTest()
        {
            var cache = new InstanceByKeyCache<string, ArrayKey<int>>();
            var key1 = new ArrayKey<int>(new[] { 1, 2, 3 });
            var key2 = new ArrayKey<int>(new[] { 3, 2, 1 });
            var value1 = cache.Get(key1, (localKey) =>
            {
                return "First";
            }, CacheDuration);
            var value2 = cache.Get(key2, (localKey) =>
            {
                return "Second";
            }, CacheDuration);
            var value3 = cache.Get(key2, (localKey) =>
            {
                return "Third";
            }, CacheDuration);
            Assert.AreEqual("First", value1);
            Assert.AreEqual("Second", value2);
            Assert.AreEqual("Second", value3);
        }

        /// <summary>
        /// Test to make sure cache gets cleared successfully when .Clear() is called 
        /// (either explicitly/manually or by an invalidator)
        /// </summary>
        [TestMethod]
        public void ClearCacheTest()
        {
            // set up cache with an initial value
            var cache = new InstanceByKeyCache<string, string>();
            var key = "randomKey";
            var cachedValue = "Initial value";

            // get value from cache and check it matches initial
            var value = cache.Get(key, (localKey) =>
            {
                return cachedValue;
            }, CacheDuration);
            Assert.AreEqual("Initial value", value);

            // clear the cache
            cache.Clear();

            // change the cached value
            cachedValue = "Changed value";

            // check that it returns the changed value
            value = cache.Get(key, (localKey) =>
            {
                return cachedValue;
            }, CacheDuration);
            Assert.AreEqual("Changed value", value);
        }

        #endregion

    }

}