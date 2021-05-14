namespace Rhythm.Caching.Core.Comparers
{

    // Namespaces.
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Compares an array of items.
    /// </summary>
    public class ArrayComparer<T> : IEqualityComparer<T[]>
    {

        #region Methods

        /// <summary>
        /// Check if the arrays are equal.
        /// </summary>
        /// <param name="x">
        /// The first array.
        /// </param>
        /// <param name="y">
        /// The second array.
        /// </param>
        /// <returns>
        /// True, if the arrays are both null, are both empty, or both have
        /// the same strings in the same order; otherwise, false.
        /// </returns>
        public bool Equals(T[] x, T[] y)
        {
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return ReferenceEquals(x, y);
            }
            if (x.Length != y.Length)
            {
                return false;
            }
            for (var i = 0; i < x.Length; i++)
            {
                if (ReferenceEquals(x[i], null) || ReferenceEquals(y[i], null))
                {
                    if (!ReferenceEquals(x[i], y[i]))
                    {
                        return false;
                    }
                }
                else if (!x[i].Equals(y[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Generates a hash code by combining all of the hash codes for the items in the array.
        /// </summary>
        /// <param name="items">
        /// The array of items.
        /// </param>
        /// <returns>
        /// The combined hash code.
        /// </returns>
        public int GetHashCode(T[] items)
        {
            if (ReferenceEquals(items, null) || !items.Any())
            {
                return 0;
            }
            else
            {
                var hashCode = default(int);
                foreach (var item in items)
                {
                    if (!ReferenceEquals(item, null))
                    {
                        hashCode ^= item.GetHashCode();
                    }
                }
                return hashCode;
            }
        }

        #endregion

    }

}