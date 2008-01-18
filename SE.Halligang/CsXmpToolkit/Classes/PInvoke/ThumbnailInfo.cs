using System;
using System.Runtime.InteropServices;

namespace SE.Halligang.CsXmpToolkit.PInvoke
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ThumbnailInfo
	{
		public FileFormat FileFormat;
		public int FullWidth;
		public int FullHeight;
		public int ThumbnailWidth;
		public int ThumbnailHeight;
		public short FullOrientation;
		public short ThumbnailOrientation;
		public IntPtr ThumbnailImage;
		public int ThumbnailSize;
		public ThumbnailFormat ThumbnailFormat;
		public byte Pad1;
		public byte Pad2;
		public byte Pad3;
	}
}
