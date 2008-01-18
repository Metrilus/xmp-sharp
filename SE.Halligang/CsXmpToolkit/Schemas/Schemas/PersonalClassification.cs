using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	/// <summary>
	/// Personal Classification Schema
	/// </summary>
	public sealed class PersonalClassification
	{
		/// <summary>
		/// Constructs a PersonalClassification object.
		/// </summary>
		/// <param name="xmp"></param>
		public PersonalClassification(Xmp xmp)
		{
			this.xmpCore = xmp.XmpCore;

			RegisterNamespace();
			Classification.RegisterNamespace();

			PropertyFlags options;

			xmpCore.GetProperty(ns, "creator", out creator, out options);
			classifications = new XmpArray<Classification>(xmp.XmpCore, ns, "classification", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<Classification>(ClassificationCallback));
		}

		private static readonly string ns = "http://ns.halligang.se/pcs/1.0/";
		/// <summary>
		/// Gets the namespace URI for the schema.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string prefix = "pcs";
		/// <summary>
		/// Gets the preferred prefix for the struct.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return prefix; }
		}

		private string creator;
		/// <summary>
		/// 
		/// </summary>
		public string Creator
		{
			get { return creator; }
			set
			{
				creator = value;
				if (creator != null)
				{
					xmpCore.SetProperty(ns, "creator", creator, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "creator");
				}
			}
		}	

		private XmpArray<Classification> classifications;
		/// <summary>
		/// 
		/// </summary>
		public XmpArray<Classification> Classifications
		{
			get { return classifications; }
		}

		protected XmpCore xmpCore;

		internal static string RegisterNamespace()
		{
			string registeredPrefix;
			XmpCore.RegisterNamespace(PersonalClassification.Namespace, PersonalClassification.PreferredPrefix, out registeredPrefix);
			return registeredPrefix;
		}

		private static void ClassificationCallback(XmpCore xmpCore, string schemaNamespace, string propertyPath,
			PropertyFlags options, XmpArrayCallbackType type, List<Classification> items, int itemIndex, Classification itemValue)
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
							// Read Subject
							string subject;
							xmpCore.GetStructField(schemaNamespace, propPath, Classification.Namespace, "subject", out subject, out options);

							// Read Type
							string typeString = null;
							xmpCore.GetStructField(schemaNamespace, propPath, Classification.Namespace, "type", out typeString, out options);
							ClassificationType classificationType;
							switch (typeString.ToLower())
							{
								case "descriptive":
									classificationType = ClassificationType.Descriptive;
									break;
								case "containing":
									classificationType = ClassificationType.Containing;
									break;
								default:
									continue;
							}

							// Read Path
							List<string> pathList = new List<string>();
							string fieldPath;
							XmpUtils.ComposeStructFieldPath(schemaNamespace, propPath, Classification.Namespace, "path", out fieldPath);
							XmpArray<string> pathArray = new XmpArray<string>(xmpCore, schemaNamespace, fieldPath, PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsOrdered, new XmpArrayCallback<string>(XmpArray.StringCallback));
							foreach (string path in pathArray)
							{
								pathList.Add(path);
							}

							// Add item
							items.Add(new Classification(subject, classificationType, pathList.ToArray()));
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

					PropertyFlags addFlags = PropertyFlags.ValueIsStruct;
					if (itemIndex < items.Count)
					{
						addFlags |= PropertyFlags.InsertBeforeItem;
					}

					// Create Classification
					xmpCore.SetArrayItem(schemaNamespace, propertyPath, itemIndex, null, addFlags);
					string structPath;
					XmpUtils.ComposeArrayItemPath(schemaNamespace, propertyPath, itemIndex, out structPath);

					// Set Subject
					if (itemValue.Subject == null)
					{
						xmpCore.DeleteStructField(schemaNamespace, structPath, Classification.Namespace, "subject");
					}
					else
					{
						xmpCore.SetStructField(schemaNamespace, structPath, Classification.Namespace, "subject", itemValue.Subject, PropertyFlags.None);
					}

					// Set Type
					switch (itemValue.Type)
					{
						case ClassificationType.Descriptive:
							xmpCore.SetStructField(schemaNamespace, structPath, Classification.Namespace, "type", "descriptive", PropertyFlags.None);
							break;
						case ClassificationType.Containing:
							xmpCore.SetStructField(schemaNamespace, structPath, Classification.Namespace, "type", "containing", PropertyFlags.None);
							break;
					}

					// Set Path
					string arrayPath;
					XmpUtils.ComposeStructFieldPath(schemaNamespace, structPath, Classification.Namespace, "path", out arrayPath);
					foreach (string path in itemValue.Path)
					{
						xmpCore.AppendArrayItem(schemaNamespace, arrayPath, PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsOrdered, path, PropertyFlags.None);
					}
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
