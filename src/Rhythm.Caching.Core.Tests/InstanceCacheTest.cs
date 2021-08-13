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
        public void CacheLockExpiresOnTimeTest()
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

        /// <summary>
        /// Test to ensure the lock timeout would never expire when the default constructor
        /// for InstanceCache is used (i.e., when no timeout is specified).
        /// </summary>
        [TestMethod]
        public void CacheLockNeverExpiresTest()
        {
            Assert.AreEqual(TimeSpan.FromMilliseconds(-1), CacheSettings.DefaultLockTimeout);
            var cache = new InstanceCache<string>();
            var startTime = DateTime.Now;
            var value = cache.Get(CacheDuration, () =>
            {
                var result = "Initial";
                var thread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        result = cache.Get(CacheDuration, () =>
                        {
                            return "Hello";
                        });
                    }
                    catch (ThreadAbortException)
                    {
                        Thread.ResetAbort();
                    }
                }));
                thread.Start();
                thread.Join(TwoSeconds);
                thread.Abort();
                return result;
            });
            var endTime = DateTime.Now;
            var diff = endTime.Subtract(startTime);
            Assert.AreEqual("Initial", value);
            Assert.IsTrue(diff.TotalSeconds >= 1.8 && diff.TotalSeconds < 3);
        }

        /// <summary>
        /// Test to make sure cache gets cleared successfully when .Clear() is called 
        /// (either explicitly/manually or by an invalidator)
        /// </summary>
        [TestMethod]
        public void ClearCacheTest()
        {
            // set up cache with an initial value
            var cache = new InstanceCache<string>();
            var cachedValue = "Initial value";

            // get value from cache and check it matches initial
            var value = cache.Get(CacheDuration, () => cachedValue);
            Assert.AreEqual("Initial value", value);

            // clear the cache
            cache.Clear();

            // change the cached value
            cachedValue = "Changed value";

            // check that it returns the changed value
            value = cache.Get(CacheDuration, () => cachedValue);
            Assert.AreEqual("Changed value", value);
        }
        #endregion

    }

}