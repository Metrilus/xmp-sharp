using System;

namespace SE.Halligang.CsXmpToolkit
{
	public enum IteratorMode : int
	{
		/// <summary>
		/// Iterate everything.
		/// </summary>
		IncludeAll		= 0,

		/// <summary>
		/// Just do the immediate children of the root, default is subtree.
		/// </summary>
		JustChildren	= 0x0100,

		/// <summary>
		/// Just do the leaf nodes, default is all nodes in the subtree.
		/// </summary>
		JustLeafNodes	= 0x0200,

		/// <summary>
		/// Return just the leaf part of the path, default is the full path.
		/// </summary>
		JustLeafName	= 0x0400,

		/// <summary>
		/// Omit all qualifiers.
		/// </summary>
		OmitQualifiers	= 0x1000,
	}
}
