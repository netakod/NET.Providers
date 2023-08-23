using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Simple.Network
{
    class DefaultObjectPipeWithSupplyControl<T> : DefaultObjectPipe<T>, IValueTaskSource, ISupplyController
    {
        private ManualResetValueTaskSourceCore<bool> _taskSourceCore;

        private short _currentTaskVersion = 0;

        public DefaultObjectPipeWithSupplyControl()
            : base()
        {
            _taskSourceCore = new ManualResetValueTaskSourceCore<bool>
            {
                RunContinuationsAsynchronously = true
            };
        }

        public ValueTask SupplyRequired()
        {
            lock (this)
            {
                if (_currentTaskVersion == -1)
                {
                    _currentTaskVersion = 0;
                    return new ValueTask();
                }

                _taskSourceCore.Reset();
                _currentTaskVersion = _taskSourceCore.Version;
                return new ValueTask(this, _taskSourceCore.Version);
            }
        }

        protected override void OnWaitTaskStart()
        {
            SetTaskCompleted(true);
        }

        public void SupplyEnd()
        {
            SetTaskCompleted(false);
        }

        private void SetTaskCompleted(bool result)
        {
            lock (this)
            {
                if (_currentTaskVersion == 0)
                {
                    _currentTaskVersion = -1;
                    return;
                }

                _taskSourceCore.SetResult(result);
                _currentTaskVersion = 0;
            }
        }

        void IValueTaskSource.GetResult(short token)
        {
            _taskSourceCore.GetResult(token);
        }

        ValueTaskSourceStatus IValueTaskSource.GetStatus(short token)
        {
            return _taskSourceCore.GetStatus(token);
        }

        void IValueTaskSource.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            _taskSourceCore.OnCompleted(continuation, state, token, flags);
        }
    }
}
