using System;

namespace SE.Halligang.CsXmpToolkit
{
	public enum IteratorSkipMode : int
	{
		/// <summary>
		/// Skip the subtree below the current node.
		/// </summary>
		SkipSubtree		= 0x0001,

		/// <summary>
		/// Skip the subtree below and remaining siblings of the current node.
		/// </summary>
		SkipSiblings	= 0x0002,
	}
}
