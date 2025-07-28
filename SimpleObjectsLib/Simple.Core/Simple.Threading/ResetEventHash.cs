using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Threading
{
	public class ResetEventHash<TToken>
	{
		private Hashtable resetEvents = new Hashtable();

		public ResetEventHash() 
        { 
        }

		protected Hashtable ResetEvents => Hashtable.Synchronized(this.resetEvents);

        public ICollection Keys => this.resetEvents.Keys;
        public ICollection Values => this.resetEvents.Values;

        public bool IsReleased(TToken token) => !this.ResetEvents.ContainsKey(token);

        public bool ContainsToken(TToken token) => this.ResetEvents.ContainsKey(token);

        public ManualResetEvent GetResetEvent(TToken token)
        {
            ManualResetEvent resetEvent;

            if (this.ResetEvents.ContainsKey(token))
            {
                resetEvent = (ManualResetEvent)this.ResetEvents[token];
            }
            else
            {
                resetEvent = new ManualResetEvent(false);
                this.ResetEvents.Add(token, resetEvent);
            }

            return resetEvent;
        }

        public virtual void Prepare(TToken token)
        {
            // GetResetEvent will only create ManualResetEvent without calling Wait on it.
            ManualResetEvent resetEvent = this.GetResetEvent(token);
        }

        public virtual bool Release(TToken token)
        {
            bool released = false;

            if (this.ResetEvents.ContainsKey(token))
            {
                ManualResetEvent resetEvent = (ManualResetEvent)this.ResetEvents[token];
                
                this.ResetEvents.Remove(token);

                resetEvent.Set();
                resetEvent.Close();

                released = true;
            }

            return released;
        }

        public virtual void DisposeResetEvent(TToken token)
        {
            if (this.ResetEvents.ContainsKey(token))
            {
                ManualResetEvent resetEvent = (ManualResetEvent)this.ResetEvents[token];
                this.ResetEvents.Remove(token);

                resetEvent.Close();
            }
        }

        public void Dispose()
        {
            TToken[] tokens = new TToken[this.resetEvents.Keys.Count];
            this.resetEvents.Keys.CopyTo(tokens, 0);

            foreach (TToken token in tokens)
                this.DisposeResetEvent(token);
        }
    }
}
