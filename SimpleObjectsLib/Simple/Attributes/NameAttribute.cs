using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
	/// <summary>
	/// Specifies a name for a property or event.
	/// </summary>
	[AttributeUsage(AttributeTargets.All)]
	public class NameAttribute : Attribute
	{
		public NameAttribute(string value)
		{
			this.Name = value;
		}

		public string Name { get; private set; }
	}
}
