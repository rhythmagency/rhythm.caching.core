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

        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor.
        /// </summary>
        static CacheSettings()
        {

            // Set the default lock timeout to be infinite.
            DefaultLockTimeout = TimeSpan.FromMilliseconds(-1);

        }

        #endregion

    }

}