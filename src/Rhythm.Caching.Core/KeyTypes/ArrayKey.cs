namespace Rhythm.Caching.Core.KeyTypes
{

    // Namespaces.
    using Rhythm.Caching.Core.Caches;
    using Rhythm.Caching.Core.Comparers;

    /// <summary>
    /// Facilitates using an array as a key in an <see cref="InstanceByKeyCache{T, TKey}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ArrayKey<T>
    {

        #region Properties

        /// <summary>
        /// The array of items.
        /// </summary>
        private T[] Items { get; set; }

        /// <summary>
        /// Use to compare arrays of items.
        /// </summary>
        private ArrayComparer<T> Comparer { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="items">
        /// The array of items.
        /// </param>
        public ArrayKey(T[] items)
        {
            Items = items;
            Comparer = new ArrayComparer<T>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Indicates whether or not this array of items is equal to the other array of items.
        /// </summary>
        /// <param name="other">
        /// The other array of items.
        /// </param>
        /// <returns>
        /// True, if the two arrays are equal; otherwise, false.
        /// </returns>
        public bool Equals(ArrayKey<T> other)
        {
            return Comparer.Equals(this.Items, other.Items);
        }

        /// <inheritdoc cref="Equals(ArrayKey{T})"/>
        public override bool Equals(object other)
        {
            return this.Equals(other as ArrayKey<T>);
        }

        /// <summary>
        /// Returns a hash code for the array.
        /// </summary>
        /// <returns>
        /// The hash code for the array.
        /// </returns>
        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this.Items);
        }

        #endregion

    }

}