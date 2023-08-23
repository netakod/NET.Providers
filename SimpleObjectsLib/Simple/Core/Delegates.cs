using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public delegate ValueTask AsyncAction();
	public delegate ValueTask AsyncAction<in T>(T obj);

	public delegate ValueTask<T> AsyncFunc<T>();
}
