using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	/// <summary>
	/// XMP Basic Schema
	/// </summary>
	public sealed class XmpBasic
	{
		/// <summary>
		/// Constructs an XmpBasic object.
		/// </summary>
		/// <param name="xmp"></param>
		public XmpBasic(Xmp xmp)
		{
			this.xmpCore = xmp.XmpCore;

			RegisterNamespace();
			Thumbnail.RegisterNamespace();

			PropertyFlags options;

			advisory = new XmpArray<string>(xmpCore, ns, "Advisory", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<string>(XmpArray.StringCallback));
			xmpCore.GetProperty(ns, "BaseURL", out baseUrl, out options);
			createDate = ReadDate("CreateDate");
			xmpCore.GetProperty(ns, "CreatorTool", out creatorTool, out options);
			identifier = new XmpArray<QualifiedIdentifier>(xmpCore, ns, "Identifier", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<QualifiedIdentifier>(QualifiedIdentifierCallback));
			xmpCore.GetProperty(ns, "Label", out label, out options);
			metadataDate = ReadDate("MetadataDate");
			modifyDate = ReadDate("ModifyDate");
			xmpCore.GetProperty(ns, "Nickname", out nickname, out options);
			rating = ReadInt("Rating");
			thumbnails = new XmpArray<Thumbnail>(xmpCore, ns, "Thumbnails", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsAlternate, new XmpArrayCallback<Thumbnail>(ThumbnailCallback));
		}

		private Nullable<DateTime> ReadDate(string propertyName)
		{
			string propertyValue;
			PropertyFlags options;
			xmpCore.GetProperty(ns, propertyName, out propertyValue, out options);
			if (propertyValue == null)
			{
				return null;
			}
			else
			{
				return XmpDateTime.XmpStringToDateTime(propertyValue);
			}
		}

		private Nullable<int> ReadInt(string propertyName)
		{
			string propertyValue;
			PropertyFlags options;
			xmpCore.GetProperty(ns, propertyName, out propertyValue, out options);
			if (propertyValue == null)
			{
				return null;
			}
			else
			{
				return Convert.ToInt32(propertyValue);
			}
		}

		private static readonly string ns = "http://ns.adobe.com/xap/1.0/";
		/// <summary>
		/// Gets the namespace URI for the schema.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string preferredPrefix = "xmp";
		/// <summary>
		/// 
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return preferredPrefix; }
		}

		private XmpArray<string> advisory;
		/// <summary>
		/// An unordered array specifying properties that were
		/// edited outside the authoring application.
		/// Each item should contain a single namespace and XPath
		/// separated by one ASCII space (U+0020).
		/// </summary>
		public XmpArray<string> Advisory
		{
			get { return advisory; }
		}

		private string baseUrl;
		/// <summary>
		/// The base URL for relative URLs in the document
		/// content. If this document contains Internet links, and
		/// those links are relative, they are relative to this base
		/// URL.
		/// This property provides a standard way for embedded
		/// relative URLs to be interpreted by tools. Web authoring
		/// tools should set the value based on their notion of where
		/// URLs will be interpreted.
		/// </summary>
		public string BaseUrl
		{
			get { return baseUrl; }
			set
			{
				baseUrl = value;
				if (baseUrl != null)
				{
					xmpCore.SetProperty(ns, "BaseUrl", baseUrl, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "BaseUrl");
				}
			}
		}

		private Nullable<DateTime> createDate;
		/// <summary>
		/// The date and time the resource was originally created.
		/// </summary>
		public Nullable<DateTime> CreateDate
		{
			get { return createDate; }
			set
			{
				createDate = value;
				if (createDate != null)
				{
					xmpCore.SetProperty(ns, "CreateDate", XmpDateTime.DateTimeToXmpString(createDate.Value, TimeZone.CurrentTimeZone), PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "CreateDate");
				}
			}
		}

		private string creatorTool;
		/// <summary>
		/// The name of the first known tool used to create the
		/// resource. If history is present in the metadata, this value
		/// should be equivalent to that of xmpMM:History's
		/// softwareAgent property.
		/// </summary>
		public string CreatorTool
		{
			get { return creatorTool; }
			set
			{
				creatorTool = value;
				if (creatorTool != null)
				{
					xmpCore.SetProperty(ns, "CreatorTool", creatorTool, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "CreatorTool");
				}
			}
		}

		private XmpArray<QualifiedIdentifier> identifier;
		/// <summary>
		/// An unordered array of text strings that unambiguously
		/// identify the resource within a given context. An array
		/// item may be qualified with xmpidq:Scheme to denote
		/// the formal identification system to which that identifier
		/// conforms.
		/// NOTE: The dc:identifier property is not used because it
		///       lacks a defined scheme qualifier and has been
		///       defined in the XMP Specification as a simple
		///       (single-valued) property.
		/// </summary>
		public XmpArray<QualifiedIdentifier> Identifier
		{
			get { return identifier; }
		}

		private string label;
		/// <summary>
		/// A word or short phrase that identifies a document as a
		/// member of a user-defined collection. Used to organize
		/// documents in a file browser.
		/// </summary>
		public string Label
		{
			get { return label; }
			set
			{
				label = value;
				if (label != null)
				{
					xmpCore.SetProperty(ns, "Label", label, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Label");
				}
			}
		}

		private Nullable<DateTime> metadataDate;
		/// <summary>
		/// The date and time that any metadata for this resource
		/// was last changed. It should be the same as or more
		/// recent than xmp:ModifyDate.
		/// </summary>
		public Nullable<DateTime> MetadataDate
		{
			get { return metadataDate; }
			set
			{
				metadataDate = value;
				if (metadataDate != null)
				{
					xmpCore.SetProperty(ns, "MetadataDate", XmpDateTime.DateTimeToXmpString(metadataDate.Value, TimeZone.CurrentTimeZone), PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "MetadataDate");
				}
			}
		}

		private Nullable<DateTime> modifyDate;
		/// <summary>
		/// The date and time the resource was last modified.
		/// NOTE: The value of this property is not necessarily the
		///	      same as the file’s system modification date
		///       because it is set before the file is saved.
		/// </summary>
		public Nullable<DateTime> ModifyDate
		{
			get { return modifyDate; }
			set
			{
				modifyDate = value;
				if (modifyDate != null)
				{
					xmpCore.SetProperty(ns, "ModifyDate", XmpDateTime.DateTimeToXmpString(modifyDate.Value, TimeZone.CurrentTimeZone), PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "ModifyDate");
				}
			}
		}

		private string nickname;
		/// <summary>
		/// A short informal name for the resource.
		/// </summary>
		public string Nickname
		{
			get { return nickname; }
			set
			{
				nickname = value;
				if (nickname != null)
				{
					xmpCore.SetProperty(ns, "Nickname", nickname, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Nickname");
				}
			}
		}

		private Nullable<int> rating;
		/// <summary>
		/// A number that indicates a document’s status relative to
		/// other documents, used to organize documents in a file
		/// browser. Values are user-defined within an applicationdefined
		/// range.
		/// </summary>
		public Nullable<int> Rating
		{
			get { return rating; }
			set
			{
				rating = value;
				if (rating != null)
				{
					xmpCore.SetProperty(ns, "Rating", rating.Value, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Rating");
				}
			}
		}

		private XmpArray<Thumbnail> thumbnails;
		/// <summary>
		/// An alternative array of thumbnail images for a file, which can differ in characteristics such as size or image encoding.
		/// </summary>
		public XmpArray<Thumbnail> Thumbnails
		{
			get { return thumbnails; }
		}

		private XmpCore xmpCore;

		internal static string RegisterNamespace()
		{
			string registeredPrefix;

			// Register namespace reqired by Identifier property
			XmpCore.RegisterNamespace("http://ns.adobe.com/xmp/Identifier/qual/1.0/", "xmpidq", out registeredPrefix);

			// Register XMP Basic namespace
			XmpCore.RegisterNamespace(XmpBasic.Namespace, XmpBasic.PreferredPrefix, out registeredPrefix);
			return registeredPrefix;
		}

		private static void QualifiedIdentifierCallback(XmpCore xmpCore, string schemaNamespace, string propertyPath,
			PropertyFlags options, XmpArrayCallbackType type, List<QualifiedIdentifier> items, int itemIndex, QualifiedIdentifier itemValue)
		{
			string idqNS = "http://ns.adobe.com/xmp/Identifier/qual/1.0/";
			string idqName = "Scheme";

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
							string scheme;
							if (xmpCore.GetQualifier(schemaNamespace, propPath, idqNS, idqName, out scheme, out options))
							{
								items.Add(new QualifiedIdentifier(propValue, scheme));
							}
							else
							{
								items.Add(new QualifiedIdentifier(propValue, null));
							}
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
					if (itemValue == null || itemValue.Value == null)
					{
						throw new ArgumentNullException("itemValue");
					}
					if (!xmpCore.DoesPropertyExist(schemaNamespace, propertyPath))
					{
						xmpCore.SetProperty(schemaNamespace, propertyPath, null, options);
					}
					PropertyFlags addFlags = PropertyFlags.None;
					if (itemIndex < items.Count)
					{
						addFlags |= PropertyFlags.InsertBeforeItem;
					} 
					xmpCore.SetArrayItem(schemaNamespace, propertyPath, itemIndex, itemValue.Value, addFlags);
					string arrayPath;
					XmpUtils.ComposeArrayItemPath(schemaNamespace, propertyPath, itemIndex, out arrayPath);
					if (itemValue.Scheme != null)
					{
						xmpCore.SetQualifier(schemaNamespace, arrayPath, idqNS, idqName, itemValue.Scheme, PropertyFlags.None);
					}
					else
					{
						xmpCore.DeleteQualifier(schemaNamespace, arrayPath, idqNS, idqName);
					}
					break;
				case XmpArrayCallbackType.Removed:
					xmpCore.DeleteArrayItem(schemaNamespace, propertyPath, itemIndex);
					break;
			}
		}

		private static void ThumbnailCallback(XmpCore xmpCore, string schemaNamespace, string propertyPath,
			PropertyFlags options, XmpArrayCallbackType type, List<Thumbnail> items, int itemIndex, Thumbnail itemValue)
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
							// Read width
							string width;
							xmpCore.GetStructField(schemaNamespace, propPath, Thumbnail.Namespace, "width", out width, out options);

							// Read height
							string height;
							xmpCore.GetStructField(schemaNamespace, propPath, Thumbnail.Namespace, "height", out height, out options);

							// Read format
							// Always JPEG
							//string format;
							//xmpCore.GetStructField(schemaNamespace, propPath, Thumbnail.Namespace, "format", out format, out options);

							// Read image
							string image;
							xmpCore.GetStructField(schemaNamespace, propPath, Thumbnail.Namespace, "image", out image, out options);

							// Add item
							items.Add(new Thumbnail(
								width == null ? 0 : Convert.ToInt32(width),
								height == null ? 0 : Convert.ToInt32(height),
								image));
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

					// Create Thumbnail
					xmpCore.SetArrayItem(schemaNamespace, propertyPath, itemIndex, null, addFlags);
					string structPath;
					XmpUtils.ComposeArrayItemPath(schemaNamespace, propertyPath, itemIndex, out structPath);

					// Set width
					xmpCore.SetStructField(schemaNamespace, structPath, Thumbnail.Namespace, "width", itemValue.Width.ToString(), PropertyFlags.None);

					// Set height
					xmpCore.SetStructField(schemaNamespace, structPath, Thumbnail.Namespace, "height", itemValue.Height.ToString(), PropertyFlags.None);

					// Set format
					xmpCore.SetStructField(schemaNamespace, structPath, Thumbnail.Namespace, "format", "JPEG", PropertyFlags.None);

					// Set image
					if (itemValue.Image == null)
					{
						xmpCore.DeleteStructField(schemaNamespace, structPath, Thumbnail.Namespace, "image");
					}
					else
					{
						xmpCore.SetStructField(schemaNamespace, structPath, Thumbnail.Namespace, "image", itemValue.Image, PropertyFlags.None);
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

		public class QualifiedIdentifier
		{
			public QualifiedIdentifier(string value, string scheme)
			{
				this.value = value;
				this.scheme = scheme;
			}

			private string value;
			/// <summary>
			/// 
			/// </summary>
			public string Value
			{
				get { return value; }
			}

			private string scheme;
			/// <summary>
			/// 
			/// </summary>
			public string Scheme
			{
				get { return scheme; }
			}
		}
	}
}