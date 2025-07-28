using Simple.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public struct ArraySegmentListPosition
	{
		public ArraySegmentListPosition(int position, int listIndex)
		{
			this.Position = position;
			this.ListIndex = listIndex;
		}

		public int Position { get; private set; }
		public int ListIndex { get; private set; }
	}
}
