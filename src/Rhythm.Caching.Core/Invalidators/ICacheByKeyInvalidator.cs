namespace Rhythm.Caching.Core.Invalidators
{

    // Namespaces.
    using System.Collections.Generic;

    /// <summary>
    /// Interface to be used by invalidators that apply to keyed caches.
    /// </summary>
    public interface ICacheByKeyInvalidator
    {

        #region Methods

        /// <summary>
        /// Invalidates the cache for the specified keys.
        /// </summary>
        /// <param name="keys">
        /// The keys to invalidate.
        /// </param>
        void InvalidateForKeys(IEnumerable<object> keys);

        #endregion

    }

}