using System;
using System.Runtime.InteropServices;

namespace SE.Halligang.CsXmpToolkit.PInvoke
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="refCon">User supplied state object.</param>
	/// <param name="buffer">Buffer containing UTF-8 encoded char array.</param>
	/// <param name="bufferSize">Size of buffer.</param>
	[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
	internal delegate int TextOutputDelegate(IntPtr refCon, IntPtr buffer, [MarshalAs(UnmanagedType.U4)] int bufferSize);
}
