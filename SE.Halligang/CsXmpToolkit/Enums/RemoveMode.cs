using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum RemoveMode
	{
		/// <summary>
		/// 
		/// </summary>
		None			= 0x0000, 

		/// <summary>
		/// Do all properties, default is just external properties.
		/// </summary>
		DoAllProperties	= 0x0001,

		/// <summary>
		/// 
		/// </summary>
		IncludeAliases	= 0x0800,
	}
}
