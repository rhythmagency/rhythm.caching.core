namespace Rhythm.Caching.Core
{

    // Namespaces.
    using System;

    /// <summary>
    /// Settings for caching.
    /// </summary>
    public class CacheSettings
    {

        #region Static Properties

        /// <summary>
        /// The duration to use by default for the timeout for locks.
        /// </summary>
        public static TimeSpan DefaultLockTimeout { get; set; }

        /// <summary>
        /// The function that gets called when a timeout expires before entering a lock. You might change this,
        /// for example, if you want to log any time a lock expires.
        /// </summary>
        public static Action<TimeSpan> FailedLockHandler { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor.
        /// </summary>
        static CacheSettings()
        {

            // Set the default lock timeout to be infinite.
            DefaultLockTimeout = TimeSpan.FromMilliseconds(-1);

            // By default, do nothing when a lock timeout expires.
            FailedLockHandler = (x) => { };

        }

        #endregion

    }

}