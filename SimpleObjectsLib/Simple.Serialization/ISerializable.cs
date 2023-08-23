using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Serialization
{
    public interface ISerializable : IWritable, IReadable
	{
    }

    public interface IWritable
    {
        int GetBufferCapacity();
        void WriteTo(SerialWriter writer, object? context);

	}

    public interface IReadable
    {
		void ReadFrom(SerialReader reader, object? context);
	}
}
