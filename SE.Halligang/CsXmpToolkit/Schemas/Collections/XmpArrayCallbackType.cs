using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	internal enum XmpArrayCallbackType
	{
		/// <summary>
		/// List is created.
		/// TODO: Read list items from XMP.
		/// </summary>
		Created,

		/// <summary>
		/// PRE: Item is about to be inserted.
		/// TODO: Update the index of all items following itemIndex.
		/// </summary>
		Insert,

		/// <summary>
		/// PRE: Item is about to be overwritten (followed by Add).
		/// TODO: Update the member status of the item being overwritten.
		/// </summary>
		Set,

		/// <summary>
		/// PRE: List is about to be cleared.
		/// TODO: Update the member status of all items.
		/// </summary>
		Clear,

		/// <summary>
		/// POST: Item was added.
		/// TODO: Update the member status of the itemValue.
		/// </summary>
		Added,

		/// <summary>
		/// POST: Item was removed.
		/// TODO: Update the member status of the itemValue.
		///       Update the index of all items following itemIndex.
		/// </summary>
		Removed,
	}
}
