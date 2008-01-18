using System;

namespace SE.Halligang.CsXmpToolkit
{
	[Flags]
	public enum OpenFlags : int
	{
		/// <summary>
		/// None specified.
		/// </summary>
		None					= 0x00000000,

		/// <summary>
		/// Open for read-only access.
		/// </summary>
		OpenForRead				= 0x00000001,

		/// <summary>
		/// 
		/// </summary>
		OpenForUpdate			= 0x00000002,
		OpenOnlyXmp				= 0x00000004,
		OpenCacheTNail			= 0x00000008,
		OpenStrictly			= 0x00000010,
		OpenUseSmartHandler		= 0x00000020,
		OpenUsePacketScanning	= 0x00000040,
		OpenLimitedScanning		= 0x00000080,
		OpenInBackground		= 0x10000000,
	}
}
