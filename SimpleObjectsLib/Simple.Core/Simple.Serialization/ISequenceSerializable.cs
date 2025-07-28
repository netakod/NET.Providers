using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Serialization
{
    public interface ISequenceSerializable : ISequenceWritable, ISequenceReadable
	{
    }

    public interface ISequenceWritable
    {
        int GetBufferCapacity();
        void WriteTo(ref SequenceWriter writer, object? context);
	}

    public interface ISequenceReadable
    {
		void ReadFrom(ref SequenceReader reader, object? context);
	}
}
