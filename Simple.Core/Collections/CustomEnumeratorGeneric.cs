using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    /// <summary>
    /// Creates custom enumerator for the specified original type T. Input enumeratorToWrap elements must be custable to U object type.
    /// </summary>
    /// <typeparam name="T">Generic original type T as.</typeparam>
    /// <typeparam name="U">Generic type U to convert to.</typeparam>
    public sealed class CustomEnumerator<T, U> : IEnumerator<U>, IEnumerator, ICloneable
        where U : T
    {
        public CustomEnumerator(IEnumerator<T> enumeratorToWrap)
        {
            if (enumeratorToWrap != null)
            {
                this.Enumerator = enumeratorToWrap;
            }
            else
            {
                throw new ArgumentException("enumeratorToWrap is null.");
            }
        }

        private IEnumerator<T> Enumerator { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool MoveNext()
        {
            return this.Enumerator.MoveNext();
        }

        public U Current
        {
            get
            {
                U value = (U)this.Enumerator.Current;
                return value;
            }
        }

        public void Reset()
        {
            this.Enumerator.Reset();
        }

        object IEnumerator.Current
        {
            get { return this.Current; }
        }

        void IDisposable.Dispose()
        {
            // No disposing originalEnumerator while it must be used for multiple times.
        }
    }
}
