using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
	public class RequestResult<T> : RequestResultBase<T>, IRequestResult<T>, IRequestResult
	{
		public static RequestResult<T> Successful = new RequestResult<T>(default(T), TaskResultInfo.Succeeded);
		public RequestResult()
		{
		}

		public RequestResult(T resultValue, TaskResultInfo resultInfo)
			: base(resultValue, resultInfo)
		{
		}

		public RequestResult(T resultValue, TaskResultInfo resultInfo, int token)
			: base(resultValue, resultInfo, token)
		{
		}

		public RequestResult(T resultValue, TaskResultInfo resultInfo, object state)
			: base(resultValue, resultInfo, state)
		{
		}

		public RequestResult(T resultValue, TaskResultInfo resultInfo, string message)
			: base(resultValue, resultInfo, message)
		{
		}

		public RequestResult(T resultValue, TaskResultInfo resultInfo, int token, string message)
			: base(resultValue, resultInfo, token, message)
		{
		}

		public RequestResult(T resultValue, TaskResultInfo resultInfo, int token, object state)
			: base(resultValue, resultInfo, token, state)
		{
		}

		public RequestResult(T resultValue, TaskResultInfo resultInfo, string message, object state)
			: base(resultValue, resultInfo, message, state)
		{
		}

		public RequestResult(T resultValue, TaskResultInfo resultInfo, int token, string message, object state)
			: base(resultValue, resultInfo, token, message, state)
		{
		}

		//public new RequestResultInfo ResultInfo
		//{
		//	get { return (RequestResultInfo)base.ResultInfo; }
		//	set { base.ResultInfo = (int)value; }
		//}
	}

	public abstract class RequestResultBase<T> : IRequestResult<T>, IRequestResult
	{
		public RequestResultBase()
		{
		}

		public RequestResultBase(T resultValue, TaskResultInfo resultInfo)
			: this(resultValue, resultInfo, null)
		{
		}

		public RequestResultBase(T resultValue, TaskResultInfo resultInfo, int token)
			: this(resultValue, resultInfo, token, null)
		{
		}

		public RequestResultBase(T resultValue, TaskResultInfo resultInfo, object state)
			: this(resultValue, resultInfo, null, state)
		{
		}

		public RequestResultBase(T resultValue, TaskResultInfo resultInfo, string message)
			: this(resultValue, resultInfo, message, null)
		{
		}

		public RequestResultBase(T resultValue, TaskResultInfo resultInfo, int token, string message)
			: this(resultValue, resultInfo, token, message, null)
		{
		}

		public RequestResultBase(T resultValue, TaskResultInfo resultInfo, int token, object state)
			: this(resultValue, resultInfo, token, null, state)
		{
		}

		public RequestResultBase(T resultValue, TaskResultInfo resultInfo, string message, object state)
			: this(resultValue, resultInfo, 0, message, state)
		{
		}

		public RequestResultBase(T resultValue, TaskResultInfo resultInfo, int token, string message, object state)
		{
			//this.Token = token;
			this.ResultValue = resultValue;
			this.ResultInfo = resultInfo;
			this.Message = message;
			//this.State = state;
		}

		public bool Succeeded
		{
			get { return this.ResultInfo == 0; }
		}

		//public int Token { get; set; }

		public T ResultValue { get; set; }
		public TaskResultInfo ResultInfo { get; set; }
		public string Message { get; set; }
		//public object State { get; set; }

		public virtual int GetBufferCapacity()
		{
			return 6 + this.Message.Length; // 1 (Succeeded) + 1 (ActionInfo) + 4 (RequestToken) + Message.Length; 
		}

		public override string ToString()
		{
			if (this.Succeeded)
			{
				if (this.ResultValue != null)
				{
					return this.ResultValue.ToString();
				}
				else
				{
					return base.ToString();
				}
			}
			else
			{
				return this.Message;
			}
		}

		object IRequestResult.ResultValue
		{
			get { return this.ResultValue; }
		}
	}

	/// <summary>
	/// Creates custom RequestResult interface for the specified T type. Input originalRequestResult Value property must be custable to T object type.
	/// </summary>
	/// <typeparam name="T">The type of the Value property.</typeparam>
	public class CustomRequestResult<T> : IRequestResult<T>, IRequestResult
	{
		private IRequestResult originalRequestResult = null;
		private T defaultValue;
		private T value;
		private bool defaultValueExists = false;
		private bool valueExists = false;

		public CustomRequestResult(IRequestResult originalRequestResult)
		{
			this.originalRequestResult = originalRequestResult;
		}

		public CustomRequestResult(IRequestResult originalRequestResult, T value)
		{
			this.originalRequestResult = originalRequestResult;
			this.valueExists = true;
			this.value = value;
		}

		public CustomRequestResult(IRequestResult originalRequestResult, T value, T defaultValue)
			: this(originalRequestResult, value)
		{
			this.defaultValueExists = true;
			this.defaultValue = defaultValue;
		}

		public bool Succeeded
		{
			get { return this.originalRequestResult.Succeeded; }
		}

		//public int Token
		//{
		//	get { return this.originalRequestResult.Token; }
		//}

		public T ResultValue
		{
			get
			{
				if (this.valueExists)
				{
					return value;
				}
				else if (this.defaultValueExists)
				{
					return Conversion.TryChangeType<T>(this.originalRequestResult.ResultValue, this.defaultValue);
				}
				else
				{
					return Conversion.TryChangeType<T>(this.originalRequestResult.ResultValue);
				}
			}
		}

		public TaskResultInfo ResultInfo
		{
			get { return this.originalRequestResult.ResultInfo; }
		}

		public string Message
		{
			get { return this.originalRequestResult.Message; }
			set { this.originalRequestResult.Message = value; }
		}

		//public object State
		//{
		//	get { return this.originalRequestResult.State; }
		//	set { this.originalRequestResult.State = value; }
		//}

		object IRequestResult.ResultValue
		{
			get { return this.ResultValue; }
		}
	}

	public interface IRequestResult<T> : IRequestResult
	{
		new T ResultValue { get; }
	}

	public interface IRequestResult
	{
		bool Succeeded { get; }
		//int Token { get; }
		object ResultValue { get; }
		TaskResultInfo ResultInfo { get; }
		string Message { get; set; }
		//object State { get; set; }
	}
}