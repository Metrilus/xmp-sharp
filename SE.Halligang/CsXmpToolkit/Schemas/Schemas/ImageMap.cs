using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	/// <summary>
	/// Image Map Schema
	/// </summary>
	public sealed class ImageMap
	{
		/// <summary>
		/// Constructs an ImageMap object.
		/// </summary>
		/// <param name="xmp"></param>
		public ImageMap(Xmp xmp)
		{
			RegisterNamespace();
			Area.RegisterNamespace();

			imageSize = new Dimensions(xmp.XmpCore, ns, "ImageSize");
			areas = new XmpArray<Area>(xmp.XmpCore, ns, "Area", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsOrdered, new XmpArrayCallback<Area>(AreaCallback));
		}

		private static readonly string ns = "http://ns.halligang.se/imagemap/1.0/";
		/// <summary>
		/// Gets the namespace URI for the schema.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string prefix = "imageMap";
		/// <summary>
		/// Gets the preferred prefix for the struct.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return prefix; }
		}

		private Dimensions imageSize;
		/// <summary>
		/// The size of the image that this map refers to.
		/// </summary>
		public Dimensions ImageSize
		{
			get { return imageSize; }
		}	

		private XmpArray<Area> areas;
		/// <summary>
		/// An ordered array of areas.
		/// </summary>
		public XmpArray<Area> Areas
		{
			get { return areas; }
		}

		internal static string RegisterNamespace()
		{
			string registeredPrefix;
			XmpCore.RegisterNamespace(ImageMap.Namespace, ImageMap.PreferredPrefix, out registeredPrefix);
			return registeredPrefix;
		}

		private void AreaCallback(XmpCore xmpCore, string schemaNamespace, string propertyPath,
			PropertyFlags options, XmpArrayCallbackType type, List<Area> items, int itemIndex, Area itemValue)
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
							// Read ShapeType
							string shapeTypeString = null;
							xmpCore.GetStructField(schemaNamespace, propPath, Area.Namespace, "type", out shapeTypeString, out options);
							ShapeType shapeType;
							switch (shapeTypeString.ToLower())
							{
								case "rect":
									shapeType = ShapeType.Rectangle;
									break;
								case "circle":
									shapeType = ShapeType.Circle;
									break;
								case "poly":
									shapeType = ShapeType.Polygon;
									break;
								default:
									continue;
							}

							// Read Cords
							List<int> cords = new List<int>();
							string fieldPath;
							XmpUtils.ComposeStructFieldPath(schemaNamespace, propPath, Area.Namespace, "cords", out fieldPath);
							XmpArray<int> cordsArray = new XmpArray<int>(xmpCore, schemaNamespace, fieldPath, PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsOrdered, new XmpArrayCallback<int>(XmpArray.IntCallback));
							foreach (int cord in cordsArray)
							{
								cords.Add(cord);
							}

							// Read Title
							XmpUtils.ComposeStructFieldPath(schemaNamespace, propPath, Area.Namespace, "title", out fieldPath);
							XmpLangAlt titleArray = new XmpLangAlt(xmpCore, schemaNamespace, fieldPath);
							List<LangEntry> title = new List<LangEntry>();
							foreach (LangEntry langEntry in titleArray)
							{
								title.Add(langEntry);
							}

							// Read Description
							XmpUtils.ComposeStructFieldPath(schemaNamespace, propPath, Area.Namespace, "description", out fieldPath);
							XmpLangAlt descriptionArray = new XmpLangAlt(xmpCore, schemaNamespace, fieldPath);
							List<LangEntry> description = new List<LangEntry>();
							foreach (LangEntry langEntry in descriptionArray)
							{
								description.Add(langEntry);
							}

							// Read Target
							string target;
							xmpCore.GetStructField(schemaNamespace, propPath, Area.Namespace, "target", out target, out options);

							// Add item
							items.Add(new Area(shapeType, cords.ToArray(), title.ToArray(), description.ToArray(), target, this));
						}
					}
					break;
				case XmpArrayCallbackType.Insert:
					for (int index = itemIndex - 1; index < items.Count - 1; index++)
					{
					}
					break;
				case XmpArrayCallbackType.Set:
					itemValue.ImageMap = null;
					break;
				case XmpArrayCallbackType.Clear:
					foreach (Area area in items)
					{
						area.ImageMap = null;
					}
					xmpCore.DeleteProperty(schemaNamespace, propertyPath);
					break;
				case XmpArrayCallbackType.Added:
					itemValue.ImageMap = this;
					if (!xmpCore.DoesPropertyExist(schemaNamespace, propertyPath))
					{
						xmpCore.SetProperty(schemaNamespace, propertyPath, null, options);
					}

					PropertyFlags addFlags = PropertyFlags.ValueIsStruct;
					if (itemIndex < items.Count)
					{
						addFlags |= PropertyFlags.InsertBeforeItem;
					}

					// Create Shape
					xmpCore.SetArrayItem(schemaNamespace, propertyPath, itemIndex, null, addFlags);
					string structPath;
					XmpUtils.ComposeArrayItemPath(schemaNamespace, propertyPath, itemIndex, out structPath);

					// Set Type
					switch (itemValue.Type)
					{
						case ShapeType.Rectangle:
							xmpCore.SetStructField(schemaNamespace, structPath, Area.Namespace, "type", "rect", PropertyFlags.None);
							break;
						case ShapeType.Circle:
							xmpCore.SetStructField(schemaNamespace, structPath, Area.Namespace, "type", "circle", PropertyFlags.None);
							break;
						case ShapeType.Polygon:
							xmpCore.SetStructField(schemaNamespace, structPath, Area.Namespace, "type", "poly", PropertyFlags.None);
							break;
					}

					// Set Cords
					string arrayPath;
					XmpUtils.ComposeStructFieldPath(schemaNamespace, structPath, Area.Namespace, "cords", out arrayPath);
					foreach (int cord in itemValue.Cords)
					{
						xmpCore.AppendArrayItem(schemaNamespace, arrayPath, PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsOrdered, cord.ToString(), PropertyFlags.None);
					}

					// Set Title
					if (itemValue.Title != null && itemValue.Title.Length > 0)
					{
						string fieldPath;
						XmpUtils.ComposeStructFieldPath(schemaNamespace, structPath, Area.Namespace, "title", out fieldPath);
						XmpLangAlt titleArray = new XmpLangAlt(xmpCore, schemaNamespace, fieldPath);
						foreach (LangEntry langEntry in itemValue.Title)
						{
							if (langEntry.Language.ToLower() == "x-default")
							{
								titleArray.Add("x-default", langEntry.Value);
								break;
							}
						}
						foreach (LangEntry langEntry in itemValue.Title)
						{
							if (langEntry.Language.ToLower() != "x-default")
							{
								titleArray.Add(langEntry.Language, langEntry.Value);
							}
						}
					}
					else
					{
						xmpCore.DeleteStructField(schemaNamespace, structPath, Area.Namespace, "title");
					}

					// Set Description
					if (itemValue.Description != null && itemValue.Description.Length > 0)
					{
						string fieldPath;
						XmpUtils.ComposeStructFieldPath(schemaNamespace, structPath, Area.Namespace, "description", out fieldPath);
						XmpLangAlt descriptionArray = new XmpLangAlt(xmpCore, schemaNamespace, fieldPath);
						foreach (LangEntry langEntry in itemValue.Description)
						{
							if (langEntry.Language.ToLower() == "x-default")
							{
								descriptionArray.Add("x-default", langEntry.Value);
								break;
							}
						}
						foreach (LangEntry langEntry in itemValue.Description)
						{
							if (langEntry.Language.ToLower() != "x-default")
							{
								descriptionArray.Add(langEntry.Language, langEntry.Value);
							}
						}
					}
					else
					{
						xmpCore.DeleteStructField(schemaNamespace, structPath, Area.Namespace, "description");
					}

					// Set Target
					if (itemValue.Target == null)
					{
						xmpCore.DeleteStructField(schemaNamespace, structPath, Area.Namespace, "target");
					}
					else
					{
						xmpCore.SetStructField(schemaNamespace, structPath, Area.Namespace, "target", itemValue.Target, PropertyFlags.None);
					}
					break;
				case XmpArrayCallbackType.Removed:
					itemValue.ImageMap = null;
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
