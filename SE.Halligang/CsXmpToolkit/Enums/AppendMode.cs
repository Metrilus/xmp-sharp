using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit
{
	[Flags]
	public enum AppendMode
	{
		/// <summary>
		/// Do all properties, default is just external properties.
		/// </summary>
		DoAllProperties		= 0x0001,

		/// <summary>
		/// Replace existing values, default is to leave them.
		/// </summary>
		ReplaceOldValues	= 0x0002,

		/// <summary>
		/// Delete properties if the new value is empty.
		/// </summary>
		DeleteEmptyValues	= 0x0004,
	}
}
