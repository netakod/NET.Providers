using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
    internal ref struct SpanElement3<T>
    {
        private readonly Span<T> span;
        private readonly int count;
		//private unsafe T* nextPtr;
		private unsafe void* nextPtr2;


		public SpanElement3(Span<T> span)
            : this(span, span.Length)
        {
        }

        public SpanElement3(Span<T> span, int count)
        {
            this.span = span;
            this.count = count;
        }

        public int Count => this.count;
        public int Length => this.span.Length;
        public ref T this[int index] { get => ref this.span[index]; }

        //public SpanItem<T> Next { get; internal set; }

        public Span<T> Span => this.span;
    }
}
