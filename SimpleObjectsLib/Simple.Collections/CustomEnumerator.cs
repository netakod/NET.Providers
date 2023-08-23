using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    /// <summary>
    /// Creates custom enumerator for the specified T type. Input enumeratorToWrap elements must be custable to T object type.
    /// </summary>
    /// <typeparam name="T">Generic type T.</typeparam>
    public sealed class CustomEnumerator<T> : IEnumerator<T>, IEnumerator, ICloneable
    {
        private IEnumerator originalEnumerator;

        public CustomEnumerator(IEnumerator enumeratorToWrap)
        {
            if (enumeratorToWrap != null)
            {
                this.originalEnumerator = enumeratorToWrap;
            }
            else
            {
                throw new ArgumentException("Enumerator to wrap is null.");
            }
        }

        private IEnumerator Enumerator
        {
            get { return this.originalEnumerator; }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool MoveNext()
        {
            return this.Enumerator.MoveNext();
        }

        public T Current
        {
            get { return (T)this.Enumerator.Current; }
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
