using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Modeling
{
	/// <summary>
	/// The class, struct, method, indexer, property, or event applicable access modifiers.
	/// </summary>
	public enum AccessModifier
	{
		Default = 0,
		Public = 1,
		Protected = 2,
		Internal = 4,
		Private = 8,
		New = 16,
		Override = 32,
		Virtual = 64,
		Abstract = 128,
		Static = 256,
		Sealed = 512,
	}

	/// <summary>
	/// The access modifiers applicable for the fields.
	/// </summary>
	public enum FieldAccessModifier
	{
		Default = 0,
		Public = 1,
		Protected = 2,
		Internal = 4,
		Private = 8,
		New = 16,
		Override = 32,
		Virtual = 64,
		Abstract = 128,
		Static = 256,
//		Sealed = 512,
		Const = 1024,
		Readonly = 2048,
	}

	public static class AccessModifierExtension
	{
		public static string ToLowerString(this AccessModifier accessModifier)
		{
			if (accessModifier == AccessModifier.Default)
				return "public";
			else
				return accessModifier.ToString("F").Replace(",", "").ToLower();
		}

		public static string ToLowerString(this FieldAccessModifier accessModifier)
		{
			if (accessModifier == FieldAccessModifier.Default)
				return "public";
			else
				return accessModifier.ToString("F").Replace(",", "").ToLower();
		}
	}
}
