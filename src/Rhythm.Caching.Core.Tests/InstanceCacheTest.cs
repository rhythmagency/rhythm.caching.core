namespace Rhythm.Caching.Core.Tests
{

    // Namespaces.
    using System;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rhythm.Caching.Core.Caches;

    /// <summary>
    /// Tests for InstanceCache.
    /// </summary>
    [TestClass]
    public class InstanceCacheTest
    {

        #region Properties

        private TimeSpan CacheDuration => TimeSpan.FromHours(1);
        private TimeSpan TwoSeconds => TimeSpan.FromSeconds(2);

        #endregion

        #region Test Methods

        /// <summary>
        /// Test to ensure you get the correct value from the cache.
        /// </summary>
        [TestMethod]
        public void GetValueTest()
        {
            var cache = new InstanceCache<string>();
            var value = cache.Get(CacheDuration, () =>
            {
                return "Hello";
            });
            Assert.AreEqual("Hello", value);
        }

        /// <summary>
        /// Test to ensure the cache expires in the expected amount of time.
        /// </summary>
        [TestMethod]
        public void CacheExpiresOnTimeTest()
        {
            var cache = new InstanceCache<string>(TwoSeconds);
            var startTime = DateTime.Now;
            var value = cache.Get(CacheDuration, () =>
            {
                var result = default(string);
                var thread = new Thread(new ThreadStart(() =>
                {
                    result = cache.Get(CacheDuration, () =>
                    {
                        return "Hello";
                    });
                }));
                thread.Start();
                thread.Join();
                return result;
            });
            var endTime = DateTime.Now;
            var diff = endTime.Subtract(startTime);
            Assert.IsNull(value);
            Assert.IsTrue(diff.TotalSeconds >= 1.8 && diff.TotalSeconds < 3);
        }

        /// <summary>
        /// Test to ensure the original cached value is returned rather than a newly supplied value.
        /// </summary>
        [TestMethod]
        public void GetCachedValueTest()
        {
            var cache = new InstanceCache<string>();
            var firstValue = "Hello";
            var value = cache.Get(CacheDuration, () => firstValue);
            value = cache.Get(CacheDuration, () => "World");
            Assert.AreEqual("Hello", value);
        }

        #endregion

    }

}