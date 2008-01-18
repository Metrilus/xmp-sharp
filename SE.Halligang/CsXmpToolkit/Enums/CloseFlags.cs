using System;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum CloseFlags : int
	{
		/// <summary>
		/// None specified.
		/// </summary>
		None			= 0x0000,

		/// <summary>
		/// Write into a temporary file and swap for crash safety.
		/// </summary>
		UpdateSafely	= 0x0001,
	}
}
