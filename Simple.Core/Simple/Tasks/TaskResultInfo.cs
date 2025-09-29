using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public enum TaskResultInfo
	{
		Succeeded = 0,
		ExceptionIsCaught = 1,
		TimeOut = 2,
		Error = 3,
		Cancelled = 4,
		UnknownRequest = 5,
		UnknownToken = 6,
		ConnectionError = 7,
		SocketError = 8,
		NotAuthorized = 9,
		NoSuchData = 10,
		NoSuchModule = 11,
		NoSuchMethod = 12,
		ExceptionIsCaughtOnRequestProcessing = 13,
		ExceptionIsCaughtOnResponseProcessing = 14,
		ExceptionIsCaughtOnMessageSending = 15,
		ExceptionIsCaughtOnMessageReceiving = 16,
		ExceptionIsCaughtOnArgsReading = 17,
		ExceptionIsCaughtOnArgsWriting = 18,
	}
}
