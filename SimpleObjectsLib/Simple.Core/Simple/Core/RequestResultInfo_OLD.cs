//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Simple
//{
//	public class RequestResultInfo : RequestInfo, IRequestResultInfo, IRequestInfo
//	{
//		public RequestResultInfo()
//		{
//		}

//		public RequestResultInfo(int requestToken, bool success, object resultValue, string message)
//			: base(requestToken, success, message)
//		{
//			this.ResultValue = resultValue;
//			//this.SendingMethod = sendingMethod;
//		}

//		public object ResultValue { get; set; }
//		//public SendingMethod SendingMethod { get; private set; }

//		public override string ToString()
//		{
//			if (this.Succeeded)
//			{
//				if (this.ResultValue != null)
//				{
//					return this.ResultValue.ToString();
//				}
//				else
//				{
//					return base.ToString();
//				}
//			}
//			else
//			{
//				return this.Message;
//			}
//		}
//	}

//	public interface IRequestResultInfo : IRequestInfo
//	{
//		object ResultValue { get; set; }
//	}
//}
