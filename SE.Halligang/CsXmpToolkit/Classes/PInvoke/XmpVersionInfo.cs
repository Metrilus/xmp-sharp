using System;
using System.Runtime.InteropServices;

namespace SE.Halligang.CsXmpToolkit.PInvoke
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XmpVersionInfo
	{
		public byte Major;
		public byte Minor;
		public byte Micro;
		public byte IsDebug;
		public int Build;
		public int Flags;
		public IntPtr Message;
	}
}
