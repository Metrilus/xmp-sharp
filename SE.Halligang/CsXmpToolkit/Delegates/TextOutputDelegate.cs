using System;
using System.Runtime.InteropServices;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// Callback that receives strings as they are sent from the Xmp Toolkit.
	/// </summary>
	/// <param name="state">User supplied state object.</param>
	/// <param name="textOutput">Text from Xmp Toolkit</param>
	public delegate void TextOutputDelegate(object state, string textOutput);
}
