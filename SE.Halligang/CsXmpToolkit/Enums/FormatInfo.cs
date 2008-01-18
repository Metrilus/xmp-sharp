using System;

namespace SE.Halligang.CsXmpToolkit
{
	[Flags]
	public enum FormatInfo : int
	{
		/// <summary>
		/// Can inject first-time XMP into an existing file.
		/// </summary>
		CanInjectXMP			= 0x00000001,

		/// <summary>
		/// Can expand XMP or other metadata in an existing file.
		/// </summary>
		CanExpand				= 0x00000002,

		/// <summary>
		/// Can copy one file to another, writing new metadata.
		/// </summary>
		CanRewrite				= 0x00000004,

		/// <summary>
		/// Can expand, but prefers in-place update.
		/// </summary>
		PrefersInPlace			= 0x00000008,

		/// <summary>
		/// Supports reconciliation between XMP and other forms.
		/// </summary>
		CanReconcile			= 0x00000010,

		/// <summary>
		/// Allows access to just the XMP, ignoring other forms.
		/// </summary>
		AllowsOnlyXMP			= 0x00000020,

		/// <summary>
		/// File handler returns raw XMP packet information.
		/// </summary>
		ReturnsRawPacket		= 0x00000040,

		/// <summary>
		/// File handler returns native thumbnail.
		/// </summary>
		ReturnsTNail			= 0x00000080,

		/// <summary>
		/// The file handler does the file open and close.
		/// </summary>
		HandlerOwnsFile			= 0x00000100,

		/// <summary>
		/// The file handler allows crash-safe file updates.
		/// </summary>
		AllowsSafeUpdate		= 0x00000200,

		/// <summary>
		/// The file format needs the XMP packet to be read-only.
		/// </summary>
		NeedsReadOnlyPacket		= 0x00000400,

		/// <summary>
		/// The file handler uses a "sidecar" file for the XMP.
		/// </summary>
		UsesSidecarXMP			= 0x00000800,
	}
}
