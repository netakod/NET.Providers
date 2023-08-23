using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Simple
{
	public static class StreamExtensions
	{
        public static byte[] GetBuffer(this Stream stream)
        {
            if (stream != null)
            {
                if (stream is MemoryStream)
                {
                    return (stream as MemoryStream).GetBuffer();
                }
                else if (stream is FileStream)
                {
                    return (stream as FileStream).GetBuffer();
                }
                else if (stream is BufferedStream)
                {
                    return (stream as BufferedStream).GetBuffer();
                }
                else
                {
                    return StreamExtensions.ToArray(stream);
                }
            }
            else
            {
                return new byte[0];
            }
        }
        

        public static byte[] ToArray(this Stream stream)
		{
			if (stream != null && stream.Length > 0)
			{
				stream.Position = 0;
				return StreamExtensions.ToArray(stream, 0, (int)stream.Length);
			}
			else
			{
				return new byte[0];
			}
		}

		public static byte[] ToArray(this Stream stream, int offset, int count)
		{
			if (stream != null)
			{
				byte[] result = new byte[stream.Length];

				stream.Read(result, offset, count);

				return result;
			}
			else
			{
				return new byte[0];
			}
		}

		public static ArraySegment<byte> ToArraySegment(this Stream stream)
		{
			return new ArraySegment<byte>(stream.GetBuffer(), 0, (int)stream.Length);
		}
	}
}
