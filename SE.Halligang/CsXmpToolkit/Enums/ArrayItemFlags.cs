using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit
{
	public enum ArrayItemFlags
	{
		/// <summary>
		/// None specified.
		/// </summary>
		None				= 0x00000000,

		/// <summary>
		/// The value is an array (RDF alt/bag/seq).
		/// </summary>
		ValueIsArray		= 0x00000200,

		/// <summary>
		/// The item order does not matter.
		/// </summary>
		ArrayIsUnordered	= ValueIsArray,

		/// <summary>
		/// Implies ValueIsArray, item order matters.
		/// </summary>
		ArrayIsOrdered		= 0x00000400,

		/// <summary>
		/// Implies ArrayIsOrdered, items are alternates.
		/// </summary>
		ArrayIsAlternate	= 0x00000800,

		/// <summary>
		/// Implies ArrayIsAlternate, items are localized text.
		/// </summary>
		ArrayIsAltText		= 0x00001000,

		/// <summary>
		/// Allow commas in item values, default is separator.
		/// </summary>
		AllowCommas			= 0x10000000,
	}
}
