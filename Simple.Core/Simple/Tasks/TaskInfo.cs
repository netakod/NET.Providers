using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public readonly struct TaskInfo<T> : ITaskInfo<T>, ITaskInfo
	{
		private readonly T resultValue;
		private readonly TaskResultInfo resultInfo;
		private readonly string message;

		public TaskInfo(T resultValue)
			: this(resultValue, info: TaskResultInfo.Succeeded)
		{
		}

		public TaskInfo(T resultValue, TaskResultInfo info)
			: this(resultValue, info, message: String.Empty)
		{
		}

		public TaskInfo(T resultValue, TaskResultInfo info, string message)
		{
			this.resultValue = resultValue;
			this.resultInfo = info;
			this.message = message;
		}

		public T ResultValue => this.resultValue;
		public TaskResultInfo ResultInfo => this.resultInfo;
		public string Message => this.message;
		public bool Succeeded => this.ResultInfo == TaskResultInfo.Succeeded;

		//object ITaskAction.ResultValue => this.ResultValue;
	}

	public struct TaskInfo : ITaskInfo
	{
		//private readonly object resultValue;
		private readonly TaskResultInfo resultInfo;
		private string message;

		public static readonly TaskInfo CompletedSuccessful = new TaskInfo(TaskResultInfo.Succeeded);

		public TaskInfo(TaskResultInfo info)
			: this(info, message: default)
		{
		}

		public TaskInfo(TaskResultInfo info, string message)
		{
			//this.resultValue = resultValue;
			this.resultInfo = info;
			this.message = message;
		}

		//public object ResultValue => this.resultValue;
		public TaskResultInfo ResultInfo => this.resultInfo;
		public string Message { get => this.message; set => this.message = value; }
		public bool Succeeded => this.ResultInfo == TaskResultInfo.Succeeded;
	}
}
