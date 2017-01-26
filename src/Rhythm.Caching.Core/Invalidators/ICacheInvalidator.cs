namespace Rhythm.Caching.Core.Invalidators
{

    // Namespaces.
    using System.Collections.Generic;

    /// <summary>
    /// Interface to be used by invalidators that apply to instance caches.
    /// </summary>
    public interface ICacheInvalidator
    {

        #region Methods

        /// <summary>
        /// Invalidates the cache unconditionally.
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Invalidates if the invalidator matches any of the specified content type aliases.
        /// </summary>
        /// <param name="aliases"></param>
        void InvalidateForAliases(IEnumerable<string> aliases);

        #endregion

    }

}