using System;

namespace SE.Halligang.CsXmpToolkit
{
	[Flags]
	public enum ParseFlags : int
	{
		None				= 0x0000,

		/// <summary>
		/// Require a surrounding x:xmpmeta element.
		/// </summary>
		RequireXmpMeta		= 0x0001,

		/// <summary>
		/// This is the not last input buffer for this parse stream.
		/// </summary>
		ParseMoreBuffers	= 0x0002,

		/// <summary>
		/// Do not reconcile alias differences, throw an exception.
		/// </summary>
		[Obsolete("Not yet implemented", true)]
		StrictAliasing		= 0x0004,
	}
}
