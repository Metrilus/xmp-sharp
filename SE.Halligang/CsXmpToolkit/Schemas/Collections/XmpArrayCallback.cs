using System;
using System.Collections.Generic;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	internal delegate void XmpArrayCallback<T>(XmpCore xmpCore, string schemaNamespace, string propertyPath,
		PropertyFlags options, XmpArrayCallbackType type, List<T> items, int itemIndex, T itemValue);
}
