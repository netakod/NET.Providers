using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
    /// <summary>
    /// Provides a faster way to store string tokens both maintaining the order that they were added and
    /// providing a fast lookup.
    /// 
    /// Based on code developed by ewbi at http://ewbi.blogs.com/develops/2006/10/uniquestringlis.html
    /// </summary>
    public sealed class UniqueStringList
    {
        const float LoadFactor = .72f;

        // Based on Golden Primes (as far as possible from nearest two powers of two)
        // at http://planetmath.org/encyclopedia/GoodHashTablePrimes.html
        static readonly int[] PrimeNumberList = new[]
            {
					// 193, 769, 3079, 12289, 49157 removed to allow quadrupling of bucket table size
					// for smaller size then reverting to doubling
					389, 1543, 6151, 24593, 98317, 196613, 393241, 786433, 1572869, 3145739, 6291469,
                    12582917, 25165843, 50331653, 100663319, 201326611, 402653189, 805306457, 1610612741
                };

        string[] stringList;
        int[] buckets;
        int bucketListCapacity;
        int stringListIndex;
        int loadLimit;
        int primeNumberListIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueStringList"/> class.
        /// </summary>
        public UniqueStringList()
        {
            bucketListCapacity = PrimeNumberList[primeNumberListIndex++];
            stringList = new string[bucketListCapacity];
            buckets = new int[bucketListCapacity];
            loadLimit = (int)(bucketListCapacity * LoadFactor);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get { return stringListIndex; }
        }

        public bool Add(string value, out int index)
        {
            var bucketIndex = GetBucketIndex(value);
            index = buckets[bucketIndex] - 1;

            if (index == -1)
            {
                stringList[stringListIndex++] = value;
                buckets[bucketIndex] = stringListIndex;

                if (stringListIndex > loadLimit)
                    Expand();

                index = stringListIndex - 1;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Expands this instance.
        /// </summary>
        private void Expand()
        {
            bucketListCapacity = PrimeNumberList[primeNumberListIndex++];
            buckets = new int[bucketListCapacity];
            var newStringlist = new string[bucketListCapacity];
            stringList.CopyTo(newStringlist, 0);
            stringList = newStringlist;
            Reindex();
        }

        /// <summary>
        /// Reindexes this instance.
        /// </summary>
        private void Reindex()
        {
            loadLimit = (int)(bucketListCapacity * LoadFactor);

            for (var stringIndex = 0; stringIndex < stringListIndex; stringIndex++)
            {
                var index = GetBucketIndex(stringList[stringIndex]);
                buckets[index] = stringIndex + 1;
            }
        }

        /// <summary>
        /// Gets the index of the bucket.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private int GetBucketIndex(string value)
        {
            var hashCode = value.GetHashCode() & 0x7fffffff;
            var bucketIndex = hashCode % bucketListCapacity;
            var increment = (bucketIndex > 1) ? bucketIndex : 1;
            var i = bucketListCapacity;

            while (0 < i--)
            {
                var stringIndex = buckets[bucketIndex];
                if (stringIndex == 0)
                    return bucketIndex;

                if (value.Equals(stringList[stringIndex - 1]))
                    return bucketIndex;

                bucketIndex = (bucketIndex + increment) % bucketListCapacity; // Probe.
            }

            throw new InvalidOperationException("Failed to locate a bucket.");
        }
    }
}
