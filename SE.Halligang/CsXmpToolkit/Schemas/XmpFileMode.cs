using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	public enum XmpFileMode
	{
		/// <summary>
		/// Reads XMP from file. Note: XMP will not be written back to file, even if changed.
		/// </summary>
		ReadOnly,

		/// <summary>
		/// Reads XMP from file, and saves when disposing object.
		/// </summary>
		ReadWrite,

		/// <summary>
		/// Creates new XMP. Note: If there already exists XMP in the file it will be overwritten.
		/// </summary>
		WriteOnly,
	}
}
