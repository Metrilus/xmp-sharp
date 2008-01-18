using System;
using System.Runtime.InteropServices;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// Callback that is called by Xmp Toolkit to get whether to abort or not.
	/// </summary>
	/// <param name="arg">User supplied state object.</param>
	/// <returns>Return true to abort or false to continue.</returns>
	[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
	public delegate bool AbortDelegate(IntPtr arg);
}
