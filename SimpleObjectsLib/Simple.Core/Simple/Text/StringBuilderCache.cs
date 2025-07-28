
using System;
using System.Text;

namespace Simple
{
    /// <summary>
    /// Provide a cached reusable instance of stringbuilder per thread.
    /// </summary>
    public static class StringBuilderCache
    {
        // The value 360 was chosen in discussion with performance experts as a compromise between using
        // as little memory per thread as possible and still covering a large part of short-lived
        // StringBuilder creations on the startup path of VS designers.
        private const int DefaultCapacity = 16; // == StringBuilder.DefaultCapacity
        public const int MaxBuilderSize = 360;

        [ThreadStatic]
        private static StringBuilder? cachedInstance = null;

        /// <summary>Get a StringBuilder for the specified capacity.</summary>
        /// <remarks>If a StringBuilder of an appropriate size is cached, it will be returned and the cache emptied.</remarks>
        public static StringBuilder Acquire(int capacity = DefaultCapacity)
        {
            if (capacity <= MaxBuilderSize)
            {
                StringBuilder? stringBuilder = cachedInstance;
                
                if (stringBuilder != null)
                {
                    // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                    // when the requested size is larger than the current capacity
                    if (capacity <= stringBuilder.Capacity)
                    {
                        cachedInstance = null;
                        stringBuilder.Clear();
                        
                        return stringBuilder;
                    }
                }
            }

            return new StringBuilder(capacity);
        }

        /// <summary>Place the specified builder in the cache if it is not too big.</summary>
        public static void Release(StringBuilder stringBuilder)
        {
            if (stringBuilder.Capacity <= MaxBuilderSize)
                cachedInstance = stringBuilder;
        }

        /// <summary>ToString() the stringbuilder, Release it to the cache, and return the resulting string.</summary>
        public static string GetStringAndRelease(StringBuilder stringBuilder)
        {
            string result = stringBuilder.ToString();
            
            Release(stringBuilder);
            
            return result;
        }
    }
}
