using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public interface ITaskInfo<T> : ITaskInfo
	{
		T ResultValue { get; }
	}

	public interface ITaskInfo
	{
		//object ResultValue { get; }
		TaskResultInfo ResultInfo { get; }
		string Message { get; }
		bool Succeeded { get; }
	}
}
