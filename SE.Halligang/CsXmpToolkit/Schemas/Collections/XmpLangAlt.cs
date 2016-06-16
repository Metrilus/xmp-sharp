using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	/// <summary>
	/// 
	/// </summary>
	public class XmpLangAlt : IEnumerable<LangEntry>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="xmpCore"></param>
		/// <param name="schemaNamespace"></param>
		/// <param name="propertyPath"></param>
		internal XmpLangAlt(XmpCore xmpCore, string schemaNamespace, string propertyPath)
		{
			this.xmpCore = xmpCore;
			this.schemaNamespace = schemaNamespace;
			this.propertyPath = propertyPath;

			dictionary = new Dictionary<string, string>();
			list = new List<string>();

			// Read from XMP
			string schemaNS;
			string propPath;
			string propValue;
			PropertyFlags options;

			XmpIterator iterator = new XmpIterator(xmpCore, schemaNamespace, propertyPath, IteratorMode.JustChildren);
			while (iterator.Next(out schemaNS, out propPath, out propValue, out options))
			{
				if (string.IsNullOrEmpty(propPath))
				{
					break;
				}
				if (propPath.IndexOf('[') >= 0 && propPath.IndexOf(']') >= 0)
				{
					string key;
					if (xmpCore.GetQualifier(schemaNamespace, propPath, xmlNS, xmlName, out key, out options))
					{
						if (!list.Contains(key))
						{
							list.Add(key);
						}
						dictionary[key] = propValue;
					}
					else
					{
						// Language qualifier did not exist. Assume x-default if list is empty, otherwise ignore value.
						if (list.Count == 0)
						{
							list.Add("x-default");
							dictionary["x-default"] = propValue;
						}
					}
				}
			}
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

		/// <summary>
		/// 
		/// </summary>
		public string DefaultValue
		{
			get
			{
				if (dictionary.ContainsKey("x-default"))
				{
					return dictionary["x-default"];
				}
				else
				{
					return null;
				}
			}
			set
			{
				this["x-default"] = value;
			}
		}
	
		private XmpCore xmpCore;
		private string schemaNamespace;
		private Dictionary<string, string> dictionary;
		private List<string> list;

		private const PropertyFlags langAltOptions = PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsOrdered | PropertyFlags.ArrayIsAlternate | PropertyFlags.ArrayIsAltText;
		private static string xmlNS = "http://www.w3.org/XML/1998/namespace";
		private static string xmlName = "lang";

		public void Add(string language, string value)
		{
			lock (dictionary)
			{
				dictionary.Add(language, value);
				list.Add(language);
				xmpCore.AppendArrayItem(schemaNamespace, propertyPath, langAltOptions, value, PropertyFlags.None);
				string arrayItemPath;
				XmpUtils.ComposeArrayItemPath(schemaNamespace, propertyPath, list.Count, out arrayItemPath);
				xmpCore.SetQualifier(schemaNamespace, arrayItemPath, xmlNS, xmlName, language, PropertyFlags.None);
			}			
		}

		public void Add(CultureInfo language, string value)
		{
			Add(language.Name, value);
		}

		public bool ContainsLanguage(string language)
		{
			lock (dictionary)
			{
				return list.Contains(language);
			}
		}

		public ICollection<string> Languages
		{
			get
			{
				lock (dictionary)
				{
					return dictionary.Keys;
				}
			}
		}

		public bool Remove(string language)
		{
			lock (dictionary)
			{
				if (list.Contains(language))
				{
					int removedIndex = list.IndexOf(language) + 1;
					xmpCore.DeleteArrayItem(schemaNamespace, propertyPath, removedIndex);
					dictionary.Remove(language);
					list.Remove(language);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public bool TryGetValue(string language, out string value)
		{
			lock (dictionary)
			{
				return dictionary.TryGetValue(language, out value);
			}
		}

		public ICollection<string> Values
		{
			get
			{
				lock (dictionary)
				{
					return dictionary.Values;
				}
			}
		}

		public string this[string language]
		{
			get
			{
				lock (dictionary)
				{
					return dictionary[language];
				}
			}
			set
			{
				lock (dictionary)
				{
					if (value == null)
					{
						list.Remove(language);
						dictionary.Remove(language);
					}
					else
					{
						dictionary[language] = value;
						if (list.Contains(language))
						{
							int updatedIndex = list.IndexOf(language) + 1;
							xmpCore.SetArrayItem(schemaNamespace, propertyPath, updatedIndex, value, PropertyFlags.None);
						}
						else
						{
							list.Add(language);
							xmpCore.AppendArrayItem(schemaNamespace, propertyPath, langAltOptions, value, PropertyFlags.None);
							string arrayItemPath;
							XmpUtils.ComposeArrayItemPath(schemaNamespace, propertyPath, list.Count, out arrayItemPath);
							xmpCore.SetQualifier(schemaNamespace, arrayItemPath, xmlNS, xmlName, language, PropertyFlags.None);
						}
					}
				}
			}
		}

		public string this[CultureInfo language]
		{
			get
			{
				return this[language.Name];
			}
			set
			{
				this[language.Name] = value;
			}
		}

		public void Clear()
		{
			lock (dictionary)
			{
				dictionary.Clear();
				list.Clear();
				xmpCore.DeleteProperty(schemaNamespace, propertyPath);
			}
		}

		public int Count
		{
			get { return list.Count; }
		}

		#region IEnumerable<LangEntry> Members

		public IEnumerator<LangEntry> GetEnumerator()
		{
			lock (dictionary)
			{
				return new LangEntryEnumerator(dictionary);
			}
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			lock (dictionary)
			{
				return new LangEntryEnumerator(dictionary);
			}
		}

		#endregion
	}
}
