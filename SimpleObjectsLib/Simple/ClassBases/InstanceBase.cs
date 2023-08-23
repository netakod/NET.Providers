using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public class InstanceBase<T> : InstanceBase where T : InstanceBase<T>, new()
    {
        public static new T Instance
        {
            get { return GetInstance<T>(); }
        }
    }
    
    public class InstanceBase
    {
        private static object? instance = null;
        private static object lockObjectInstance = new object();

        public static object? Instance
        {
            get { return instance; }
        }

        protected static T GetInstance<T>() where T : new()
        {
            lock (lockObjectInstance)
            {
                if (instance == null)
                    instance = new T();
            }

            return (T)instance;
        }
    }
}
