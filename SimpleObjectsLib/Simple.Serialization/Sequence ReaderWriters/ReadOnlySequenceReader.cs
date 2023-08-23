using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Buffers;

namespace Simple.Serialization
{
	public class ReadOnlySequenceReader : ArraySegmentListReader, ISequenceReader
	{
		public ReadOnlySequenceReader(ref ReadOnlySequence<byte> sequence)
			: base(sequence.ToArraySegmentList())
		{
		}
	}
}
