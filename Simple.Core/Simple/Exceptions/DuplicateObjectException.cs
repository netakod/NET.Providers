using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Simple
{
	public class DuplicateObjectException : Exception
	{
		public DuplicateObjectException()
		{
		}

		public DuplicateObjectException(string message)
			: base(message)
		{
		}

		public DuplicateObjectException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public DuplicateObjectException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
