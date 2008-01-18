using System;
using System.Collections;
using System.Collections.Generic;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	public class XmpArray<T> : IEnumerable<T>
	{
		internal XmpArray(XmpCore xmpCore, string schemaNamespace, string propertyPath, PropertyFlags options, XmpArrayCallback<T> callback)
		{
			if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}

			this.xmpCore = xmpCore;
			this.schemaNamespace = schemaNamespace;
			this.propertyPath = propertyPath;
			this.options = options;
			this.callback = callback;

			list = new List<T>();

			callback(xmpCore, schemaNamespace, propertyPath, options, XmpArrayCallbackType.Created, list, -1, default(T));
		}

		private string propertyPath;
		/// <summary>
		/// 
		/// </summary>
		internal string PropertyPath
		{
			get { return propertyPath; }
			set { propertyPath = value; }
		}	

		private XmpCore xmpCore;
		private string schemaNamespace;
		private PropertyFlags options;
		private XmpArrayCallback<T> callback;
		private List<T> list;

		public int IndexOf(T item)
		{
			lock (list)
			{
				return list.IndexOf(item);
			}
		}

		public void Insert(int index, T item)
		{
			bool add = false;
			lock (list)
			{
				if (index < 0 || index > list.Count)
				{
					throw new IndexOutOfRangeException();
				}

				if (index == list.Count)
				{
					add = true;
				}
			}

			if (add)
			{
				Add(item);
			}
			else
			{
				callback(xmpCore, schemaNamespace, propertyPath, options, XmpArrayCallbackType.Insert, list, index + 1, item);
				list.Insert(index, item);
				callback(xmpCore, schemaNamespace, propertyPath, options, XmpArrayCallbackType.Added, list, index + 1, item);
			}
		}

		public void RemoveAt(int index)
		{
			lock (list)
			{
				T removedItem = list[index];
				list.RemoveAt(index);
				callback(xmpCore, schemaNamespace, propertyPath, options, XmpArrayCallbackType.Removed, list, index + 1, removedItem);
			}
		}

		public T this[int index]
		{
			get
			{
				lock (list)
				{
					return list[index];
				}
			}
			set
			{
				lock (list)
				{
					if (value == null)
					{
						RemoveAt(index);
					}
					else
					{
						if (index < list.Count)
						{
							callback(xmpCore, schemaNamespace, propertyPath, options, XmpArrayCallbackType.Set, list, index + 1, list[index]);
						}
						list[index] = value;
						callback(xmpCore, schemaNamespace, propertyPath, options, XmpArrayCallbackType.Added, list, index + 1, value);
					}
				}
			}
		}

		public void Add(T item)
		{
			lock (list)
			{
				list.Add(item);
				callback(xmpCore, schemaNamespace, propertyPath, options, XmpArrayCallbackType.Added, list, list.Count, item);
			}
		}

		public void Clear()
		{
			lock (list)
			{
				callback(xmpCore, schemaNamespace, propertyPath, options, XmpArrayCallbackType.Clear, list, -1, default(T));
				list.Clear();
			}
		}

		public bool Contains(T item)
		{
			lock (list)
			{
				return list.Contains(item);
			}
		}

		public int Count
		{
			get
			{
				lock (list)
				{
					return list.Count;
				}
			}
		}

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		#endregion
	}

	/// <summary>
	/// 
	/// </summary>
	internal static class XmpArray
	{
		public static void StringCallback(XmpCore xmpCore, string schemaNamespace, string propertyPath,
			PropertyFlags options, XmpArrayCallbackType type, List<string> items, int itemIndex, string itemValue)
		{
			switch (type)
			{
				case XmpArrayCallbackType.Created:
					string schemaNS;
					string propPath;
					string propValue;

					XmpIterator iterator = new XmpIterator(xmpCore, schemaNamespace, propertyPath, IteratorMode.JustChildren);
					while (iterator.Next(out schemaNS, out propPath, out propValue, out options))
					{
						if (propPath.IndexOf('[') >= 0 && propPath.IndexOf(']') >= 0)
						{
							items.Add(propValue);
						}
					}
					break;
				case XmpArrayCallbackType.Insert:
					for (int index = itemIndex - 1; index < items.Count - 1; index++)
					{
					}
					break;
				case XmpArrayCallbackType.Set:
					break;
				case XmpArrayCallbackType.Clear:
					xmpCore.DeleteProperty(schemaNamespace, propertyPath);
					break;
				case XmpArrayCallbackType.Added:
					if (!xmpCore.DoesPropertyExist(schemaNamespace, propertyPath))
					{
						xmpCore.SetProperty(schemaNamespace, propertyPath, null, options);
					}
					PropertyFlags addFlags = PropertyFlags.None;
					if (itemIndex < items.Count)
					{
						addFlags |= PropertyFlags.InsertBeforeItem;
					}
					xmpCore.SetArrayItem(schemaNamespace, propertyPath, itemIndex, itemValue, addFlags);
					break;
				case XmpArrayCallbackType.Removed:
					if (items.Count == 0)
					{
						xmpCore.DeleteProperty(schemaNamespace, propertyPath);
					}
					else
					{
						xmpCore.DeleteArrayItem(schemaNamespace, propertyPath, itemIndex);
					}
					break;
			}
		}

		public static void DateCallback(XmpCore xmpCore, string schemaNamespace, string propertyPath,
			PropertyFlags options, XmpArrayCallbackType type, List<DateTime> items, int itemIndex, DateTime itemValue)
		{
			switch (type)
			{
				case XmpArrayCallbackType.Created:
					string schemaNS;
					string propPath;
					string propValue;

					XmpIterator iterator = new XmpIterator(xmpCore, schemaNamespace, propertyPath, IteratorMode.JustChildren);
					while (iterator.Next(out schemaNS, out propPath, out propValue, out options))
					{
						if (propPath.IndexOf('[') >= 0 && propPath.IndexOf(']') >= 0)
						{							
							items.Add(XmpDateTime.XmpStringToDateTime(propValue));
						}
					}
					break;
				case XmpArrayCallbackType.Insert:
					for (int index = itemIndex - 1; index < items.Count - 1; index++)
					{
					}
					break;
				case XmpArrayCallbackType.Set:
					break;
				case XmpArrayCallbackType.Clear:
					xmpCore.DeleteProperty(schemaNamespace, propertyPath);
					break;
				case XmpArrayCallbackType.Added:
					if (!xmpCore.DoesPropertyExist(schemaNamespace, propertyPath))
					{
						xmpCore.SetProperty(schemaNamespace, propertyPath, null, options);
					}
					PropertyFlags addFlags = PropertyFlags.None;
					if (itemIndex < items.Count)
					{
						addFlags |= PropertyFlags.InsertBeforeItem;
					}
					xmpCore.SetArrayItem(schemaNamespace, propertyPath, itemIndex, XmpDateTime.DateTimeToXmpString(itemValue, TimeZone.CurrentTimeZone), addFlags);
					break;
				case XmpArrayCallbackType.Removed:
					if (items.Count == 0)
					{
						xmpCore.DeleteProperty(schemaNamespace, propertyPath);
					}
					else
					{
						xmpCore.DeleteArrayItem(schemaNamespace, propertyPath, itemIndex);
					}
					break;
			}
		}

		public static void IntCallback(XmpCore xmpCore, string schemaNamespace, string propertyPath,
			PropertyFlags options, XmpArrayCallbackType type, List<int> items, int itemIndex, int itemValue)
		{
			switch (type)
			{
				case XmpArrayCallbackType.Created:
					string schemaNS;
					string propPath;
					string propValue;

					XmpIterator iterator = new XmpIterator(xmpCore, schemaNamespace, propertyPath, IteratorMode.JustChildren);
					while (iterator.Next(out schemaNS, out propPath, out propValue, out options))
					{
						if (propPath.IndexOf('[') >= 0 && propPath.IndexOf(']') >= 0)
						{
							items.Add(Convert.ToInt32(propValue));
						}
					}
					break;
				case XmpArrayCallbackType.Insert:
					for (int index = itemIndex - 1; index < items.Count - 1; index++)
					{
					}
					break;
				case XmpArrayCallbackType.Set:
					break;
				case XmpArrayCallbackType.Clear:
					xmpCore.DeleteProperty(schemaNamespace, propertyPath);
					break;
				case XmpArrayCallbackType.Added:
					if (!xmpCore.DoesPropertyExist(schemaNamespace, propertyPath))
					{
						xmpCore.SetProperty(schemaNamespace, propertyPath, null, options);
					}
					PropertyFlags addFlags = PropertyFlags.None;
					if (itemIndex < items.Count)
					{
						addFlags |= PropertyFlags.InsertBeforeItem;
					}
					xmpCore.SetArrayItem(schemaNamespace, propertyPath, itemIndex, itemValue.ToString(), addFlags);
					break;
				case XmpArrayCallbackType.Removed:
					if (items.Count == 0)
					{
						xmpCore.DeleteProperty(schemaNamespace, propertyPath);
					}
					else
					{
						xmpCore.DeleteArrayItem(schemaNamespace, propertyPath, itemIndex);
					}
					break;
			}
		}
	}
}
