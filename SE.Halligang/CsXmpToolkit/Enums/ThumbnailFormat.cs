using System;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// Specifies the format of the embedded thumbnail.
	/// </summary>
	public enum ThumbnailFormat : byte
	{
		/// <summary>
		/// The thumbnail data has an unknown format.
		/// </summary>
		Unknown		= 0,

		/// <summary>
		/// The thumbnail data is a JPEG stream, presumably compressed.
		/// </summary>
		Jpeg		= 1,

		/// <summary>
		/// The thumbnail data is a TIFF stream, presumably uncompressed.
		/// </summary>
		Tiff		= 2,

		/// <summary>
		/// The thumbnail data is in the format of Photoshop Image Resource 1036.
		/// </summary>
		Photoshop	= 3,
	}
}
