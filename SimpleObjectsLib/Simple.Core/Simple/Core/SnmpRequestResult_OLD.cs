//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Simple
//{
//    public class SnmpRequestResult<T> : ISnmpRequestResult<T>, ISnmpRequestResult
//    {
//        public SnmpRequestResult()
//            : this(default(T))
//        {
//        }
        
//        public SnmpRequestResult(T value)
//            : this(value, RequestActionResult.RequestSucceeded)
//        {
//        }
        
//        public SnmpRequestResult(T value, RequestActionResult actionResult)
//        {
//            this.Value = value;
//            this.DeclaredValueType = typeof(T);
//            this.ActionResult = actionResult;
//        }

//        public bool RequestSucceeded
//        {
//            get { return this.ActionResult == RequestActionResult.RequestSucceeded; }
//        }

//        public T Value { get; set; }
//        public Type DeclaredValueType { get; set; }
//        public RequestActionResult ActionResult { get; set; }

//        object ISnmpRequestResult.Value
//        {
//            get { return this.Value; }
//        }
//    }

//    public class SnmpRequestResult : ISnmpRequestResult
//    {
//        public SnmpRequestResult()
//            : this(null)
//        {
//        }

//        public SnmpRequestResult(object value)
//            : this(value, RequestActionResult.RequestSucceeded)
//        {
//        }

//        public SnmpRequestResult(RequestActionResult actionResult)
//            : this(null, actionResult)
//        {
//        }

//        public SnmpRequestResult(object value, RequestActionResult actionResult)
//            : this(value, actionResult, value != null ? value.GetType() : typeof(void))
//        {
//        }

//        public SnmpRequestResult(object value, RequestActionResult actionResult, Type declaredValueType)
//        {
//            this.Value = value;
//            this.DeclaredValueType = declaredValueType;
//            this.ActionResult = actionResult;
//        }

//        public bool RequestSucceeded
//        {
//            get { return this.ActionResult == RequestActionResult.RequestSucceeded; }
//        }

//        public object Value { get; set; }
//        public Type DeclaredValueType { get; set; }
//        public RequestActionResult ActionResult { get; set; }
//    }

//    /// <summary>
//    /// Creates custom SnmpRequestResult interface for the specified T type. Input originalRequestResult Value property must be custable to T object type.
//    /// </summary>
//    /// <typeparam name="T">The type of the Value property.</typeparam>
//    public class CustomSnmpRequestResult<T> : ISnmpRequestResult<T>, ISnmpRequestResult
//    {
//        private ISnmpRequestResult originalRequestResult = null;
//        private T defaultValue;
//        private T value;
//        private bool defaultValueExists = false;
//        private bool valueExists = false;

//        public CustomSnmpRequestResult(ISnmpRequestResult originalRequestResult)
//        {
//            this.originalRequestResult = originalRequestResult;
//        }

//        public CustomSnmpRequestResult(ISnmpRequestResult originalRequestResult, T value)
//        {
//            this.originalRequestResult = originalRequestResult;
//            this.valueExists = true;
//            this.value = value;
//        }

//        public CustomSnmpRequestResult(ISnmpRequestResult originalRequestResult, T value, T defaultValue)
//            : this(originalRequestResult, value)
//        {
//            this.defaultValueExists = true;
//            this.defaultValue = defaultValue;
//        }

//        public bool RequestSucceeded 
//        {
//            get { return this.originalRequestResult.RequestSucceeded; }
//        }

//        public Type DeclaredValueType 
//        { 
//            get { return typeof(T); }
//        }

//        public T Value 
//        {
//            get 
//            {
//                if (this.valueExists)
//                {
//                    return value;
//                }
//                else if (this.defaultValueExists)
//                {
//                    return Conversion.TryChangeType<T>(this.originalRequestResult.Value, this.defaultValue);
//                }
//                else
//                {
//                    return Conversion.TryChangeType<T>(this.originalRequestResult.Value);
//                }
//            }
//        }

//        public RequestActionResult ActionResult 
//        {
//            get { return this.originalRequestResult.ActionResult; }
//        }

//        object ISnmpRequestResult.Value
//        {
//            get { return this.Value; }
//        }

//    }

//    public enum RequestActionResult
//    {
//        RequestSucceeded = 1,
//        NoSuchData = 2,
//        ExeptionIsCaught = 3,
//        TimedOut = 4
//    }

//    public interface ISnmpRequestResult<T> : ISnmpRequestResult
//    {
//        new T Value { get; }
//    }

//    public interface ISnmpRequestResult
//    {
//        bool RequestSucceeded { get; }
//        Type DeclaredValueType { get; }
//        object Value { get; }
//        RequestActionResult ActionResult { get; }
//    }
//}
