using System;
using System.Runtime.InteropServices;

namespace SE.Halligang.CsXmpToolkit.PInvoke
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct PacketInfo
	{
		public long Offset;
		public int Length;
		public int PadSize;
		public byte CharForm;
		public byte Writeable;
		public byte Pad1;
		public byte Pad2;
	}
}
