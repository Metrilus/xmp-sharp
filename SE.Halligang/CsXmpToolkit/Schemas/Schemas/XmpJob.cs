using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	/// <summary>
	/// XMP Basic Job Ticket Schema
	/// </summary>
	public sealed class XmpJob
	{
		/// <summary>
		/// Constructs an ImageMap object.
		/// </summary>
		/// <param name="xmp"></param>
		public XmpJob(Xmp xmp)
		{
			RegisterNamespace();
			Job.RegisterNamespace();

			jobRef = new XmpArray<Job>(xmp.XmpCore, ns, "JobRef", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<Job>(JobCallback));
		}

		private static readonly string ns = "http://ns.adobe.com/xap/1.0/bj/";
		/// <summary>
		/// Gets the namespace URI for the schema.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string prefix = "xmpBJ";
		/// <summary>
		/// Gets the preferred prefix for the struct.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return prefix; }
		}

		private XmpArray<Job> jobRef;
		/// <summary>
		/// 
		/// </summary>
		public XmpArray<Job> JobRef
		{
			get { return jobRef; }
		}

		internal static string RegisterNamespace()
		{
			string registeredPrefix;
			XmpCore.RegisterNamespace(XmpJob.Namespace, XmpJob.PreferredPrefix, out registeredPrefix);
			return registeredPrefix;
		}

		private static void JobCallback(XmpCore xmpCore, string schemaNamespace, string propertyPath,
			PropertyFlags options, XmpArrayCallbackType type, List<Job> items, int itemIndex, Job itemValue)
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
							// Read name
							string name;
							xmpCore.GetStructField(schemaNamespace, propPath, Job.Namespace, "name", out name, out options);

							// Read id
							string id;
							xmpCore.GetStructField(schemaNamespace, propPath, Job.Namespace, "id", out id, out options);

							// Read url
							string url;
							xmpCore.GetStructField(schemaNamespace, propPath, Job.Namespace, "url", out url, out options);

							// Add item
							items.Add(new Job(name, id, url));
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

					// Create Job
					xmpCore.SetArrayItem(schemaNamespace, propertyPath, itemIndex, null, addFlags);
					string structPath;
					XmpUtils.ComposeArrayItemPath(schemaNamespace, propertyPath, itemIndex, out structPath);

					// Set name
					if (itemValue.Name == null)
					{
						xmpCore.DeleteStructField(schemaNamespace, structPath, Job.Namespace, "name");
					}
					else
					{
						xmpCore.SetStructField(schemaNamespace, structPath, Job.Namespace, "name", itemValue.Name, PropertyFlags.None);
					}

					// Set id
					if (itemValue.ID == null)
					{
						xmpCore.DeleteStructField(schemaNamespace, structPath, Job.Namespace, "id");
					}
					else
					{
						xmpCore.SetStructField(schemaNamespace, structPath, Job.Namespace, "id", itemValue.ID, PropertyFlags.None);
					}

					// Set url
					if (itemValue.Url == null)
					{
						xmpCore.DeleteStructField(schemaNamespace, structPath, Job.Namespace, "url");
					}
					else
					{
						xmpCore.SetStructField(schemaNamespace, structPath, Job.Namespace, "url", itemValue.Url, PropertyFlags.None);
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
