using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using System.Runtime.CompilerServices;

namespace Simple.Serialization
{
	/// <summary>
	/// The base class for object that need to be serialized/deserialized with <see cref="ISerializable"/> interface.
	/// </summary>
	public abstract class SerializableObject : ISerializable, System.Runtime.Serialization.ISerializable
    {
		public static System.Runtime.Serialization.ISerializable Empty = new EmptySerializableObject();

		public SerializableObject() { }

		//public SerializableObject(SerializationReader reader)
		//{
		//	this.ReadFrom(reader);
		//}

		public virtual int GetBufferCapacity() => 4;

		public abstract void WriteTo(SerialWriter writer, object? context);
        public abstract void ReadFrom(SerialReader reader, object? context);

		protected virtual int GetStringCapacity(string value)
		{
			return String.IsNullOrEmpty(value) ? 1 : value.Length + 2;
		}

		protected virtual int GetStringCapacity(IEnumerable<string> values)
		{
			int result = 4; // 4 bytes for value length

			foreach (string item in values)
				result += GetStringCapacity(item);

			return result;
		}

		public virtual SerialWriter CreateWriter(Encoding characterEncoding) => new SerialWriter(characterEncoding);

		/// <summary>
		/// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
		/// <param name="context">The destination  <see cref="StreamingContext"/> for this serialization.</param>
		/// <exception cref="SecurityException">The caller does not have the required permission.</exception>
		[SecurityCritical]
		void System.Runtime.Serialization.ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			BufferSequenceWriter bufferSequence = new BufferSequenceWriter();
			SerialWriter writer = new SerialWriter(bufferSequence);
			this.WriteTo(writer, context);
			info.AddValue("data", bufferSequence.ToArray());
		}

		private class EmptySerializableObject : SerializableObject, System.Runtime.Serialization.ISerializable
		{
			public EmptySerializableObject() { }

			public override void WriteTo(SerialWriter writer, object? context = null) { }
			public override void ReadFrom(SerialReader reader, object? context = null) { }

			public override int GetBufferCapacity() => 0;
		}
	}
}
