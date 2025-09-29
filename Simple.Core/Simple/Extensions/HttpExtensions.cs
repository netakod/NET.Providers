using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace Simple
{
	public static class HttpExtensions
	{
		public static async ValueTask<string> GetResponseTextAsync(this HttpWebResponse webResponse)
		{
			string result = String.Empty;
			StreamReader? streamReader = null;

			try
			{
				streamReader = new StreamReader(webResponse.GetResponseStream());
				result = await streamReader.ReadToEndAsync();
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				if (streamReader != null)
				{
					streamReader.Close();
					streamReader.Dispose();
				}

				if (webResponse != null)
				{
					webResponse.Close();
					(webResponse as IDisposable).Dispose();
				}
			}

			return result;
		}
	}
}
