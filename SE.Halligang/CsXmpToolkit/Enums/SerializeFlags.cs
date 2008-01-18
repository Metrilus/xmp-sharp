using System;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum SerializeFlags : int
	{
		None					= 0x0000,

		/// <summary>
		/// Omit the XML packet wrapper.
		/// </summary>
		OmitPacketWrapper		= 0x0010,

		/// <summary>
		/// Defat is a writeable packet.
		/// </summary>
		ReadOnlyPacket			= 0x0020,

		/// <summary>
		/// Use a compact form of RDF.
		/// </summary>
		UseCompactFormat		= 0x0040,

		/// <summary>
		/// Include a padding allowance for a thumbnail image.
		/// </summary>
		IncludeThumbnailPad		= 0x0100,

		/// <summary>
		/// The padding parameter is the overall packet length.
		/// </summary>
		ExactPacketLength		= 0x0200,

		/// <summary>
		/// Show aliases as XML comments.
		/// </summary>
		WriteAliasComments		= 0x0400,

		/// <summary>
		/// Omit all formatting whitespace.
		/// </summary>
		OmitAllFormatting		= 0x0800,

		/// <summary>
		/// ! Don't use directly, see the combined values below!
		/// </summary>
		LittleEndian			= 0x0001,

		/// <summary>
		/// ! Don't use directly, see the combined values below!
		/// </summary>
		UTF16					= 0x0002,

		/// <summary>
		/// ! Don't use directly, see the combined values below!
		/// </summary>
		UTF32					= 0x0004,

		/// <summary>
		/// 
		/// </summary>
		EncodingMask			= 0x0007,

		/// <summary>
		/// 
		/// </summary>
		EncodeUTF8				= 0,

		/// <summary>
		/// 
		/// </summary>
		EncodeUTF16Big			= UTF16,

		/// <summary>
		/// 
		/// </summary>
		EncodeUTF16Little		= UTF16 | LittleEndian,

		/// <summary>
		/// 
		/// </summary>
		EncodeUTF32Big			= UTF32,

		/// <summary>
		/// 
		/// </summary>
		EncodeUTF32Little		= UTF32 | LittleEndian,
	}
}
