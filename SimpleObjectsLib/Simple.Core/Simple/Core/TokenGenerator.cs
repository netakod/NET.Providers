using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public class TokenGenerator<TKey>
	{
		private readonly object lockToken = new object();
		private TKey maxToken = Operator<TKey>.Zero; 
		private HashSet<TKey> freeTokens = new HashSet<TKey>();

		public TokenGenerator()
		{
		}

		public TKey Generate()
		{
			TKey token;

			lock (this.lockToken)
			{
				if (this.freeTokens.Count > 0)
				{
					token = this.freeTokens.ElementAt(0);
					this.freeTokens.Remove(token);

					if (this.freeTokens.Count == 0)
						this.maxToken = Operator<TKey>.Zero;
				}
				else
				{
					token = this.maxToken;
					this.maxToken = Operator<TKey>.Add(this.maxToken, Operator<TKey>.One);
				}
			}

			return token;
		}

		public void Release(TKey token)
		{
			lock (this.lockToken)
			{
				//if (Operator<TKey>.Equal((TKey)((object)(this.freeTokens.Count)), this.maxToken))
				//{
				//	this.Reset();
				//}
				//else
				//{
					this.freeTokens.Add(token);
				//}
			}
		}

		private void Reset()
		{
			this.freeTokens.Clear();
			this.maxToken = Operator<TKey>.Zero;
		}
	}
}
