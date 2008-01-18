using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	/// <summary>
	/// Dublic Core Schema
	/// </summary>
	public sealed class DublinCore
	{
		/// <summary>
		/// Constructs a DublinCore object.
		/// </summary>
		/// <param name="xmp"></param>
		public DublinCore(Xmp xmp)
		{
			this.xmpCore = xmp.XmpCore;
			this.common = xmp.CommonFields.DublinCore;

			PropertyFlags options;

			contributor = new XmpArray<string>(xmpCore, ns, "contributor", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<string>(XmpArray.StringCallback));
			xmpCore.GetProperty(ns, "coverage", out coverage, out options);
			common.Creator = new XmpArray<string>(xmpCore, ns, "creator", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsOrdered, new XmpArrayCallback<string>(XmpArray.StringCallback));
			date = new XmpArray<DateTime>(xmpCore, ns, "date", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsOrdered, new XmpArrayCallback<DateTime>(XmpArray.DateCallback));
			common.Description = new XmpLangAlt(xmpCore, ns, "description");
			xmpCore.GetProperty(ns, "format", out format, out options);
			xmpCore.GetProperty(ns, "identifier", out identifier, out options);
			language = new XmpArray<string>(xmpCore, ns, "language", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<string>(XmpArray.StringCallback));
			publisher = new XmpArray<string>(xmpCore, ns, "publisher", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<string>(XmpArray.StringCallback));
			relation = new XmpArray<string>(xmpCore, ns, "relation", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<string>(XmpArray.StringCallback));
			common.Rights = new XmpLangAlt(xmpCore, ns, "rights");
			xmpCore.GetProperty(ns, "source", out source, out options);
			common.Subject = new XmpArray<string>(xmpCore, ns, "subject", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<string>(XmpArray.StringCallback));
			common.Title = new XmpLangAlt(xmpCore, ns, "title");
			xmpCore.GetProperty(ns, "type", out type, out options);
		}

		private static readonly string ns = "http://purl.org/dc/elements/1.1/";
		/// <summary>
		/// Gets the namespace URI for the schema.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string preferredPrefix = "dc";
		/// <summary>
		/// Gets the preferred prefix for the schema.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return preferredPrefix; }
		}

		private XmpArray<string> contributor;
		/// <summary>
		/// Contributors to the resource (other than the authors).
		/// </summary>
		public XmpArray<string> Contributor
		{
			get { return contributor; }
		}

		private string coverage;
		/// <summary>
		/// The extent or scope of the resource.
		/// </summary>
		public string Coverage
		{
			get { return coverage; }
			set
			{
				coverage = value;
				if (coverage != null)
				{
					xmpCore.SetProperty(ns, "coverage", coverage, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "coverage");
				}				
			}
		}

		/// <summary>
		/// The authors of the resource (listed in order of precedence, if significant).
		/// </summary>
		public XmpArray<string> Creator
		{
			get { return common.Creator; }
		}

		private XmpArray<DateTime> date;
		/// <summary>
		/// Date(s) that something interesting happened to the resource.
		/// </summary>
		public XmpArray<DateTime> Date
		{
			get { return date; }
		}

		/// <summary>
		/// A textual description of the content of the resource. Multiple values may be present for different languages.
		/// </summary>
		public XmpLangAlt Description
		{
			get { return common.Description; }
		}

		private string format;
		/// <summary>
		/// The file format used when saving the resource. Tools and applications should set 
		/// this property to the save format of the data. It may include appropriate qualifiers.
		/// </summary>
		public string Format
		{
			get { return format; }
			set
			{
				format = value;
				if (format != null)
				{
					xmpCore.SetProperty(ns, "format", format, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "format");
				}
			}
		}

		private string identifier;
		/// <summary>
		/// Unique identifier of the resource.
		/// </summary>
		[Obsolete("Use XmpBasic.Identifier instead.")]
		public string Identifier
		{
			get { return identifier; }
			set
			{
				identifier = value;
				if (identifier != null)
				{
					xmpCore.SetProperty(ns, "identifier", identifier, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "identifier");
				}
			}
		}

		private XmpArray<string> language;
		/// <summary>
		/// An unordered array specifying the languages used in the resource.
		/// </summary>
		public XmpArray<string> Language
		{
			get { return language; }
		}

		private XmpArray<string> publisher;
		/// <summary>
		/// Publishers.
		/// </summary>
		public XmpArray<string> Publisher
		{
			get { return publisher; }
		}

		private XmpArray<string> relation;
		/// <summary>
		/// Relationships to other documents.
		/// </summary>
		public XmpArray<string> Relation
		{
			get { return relation; }
		}

		/// <summary>
		/// Informal rights statement, selected by language.
		/// </summary>
		public XmpLangAlt Rights
		{
			get { return common.Rights; }
		}

		private string source;
		/// <summary>
		/// Unique identifier of the work from which this resource was derived.
		/// </summary>
		public string Source
		{
			get { return source; }
			set
			{
				source = value;
				if (source != null)
				{
					xmpCore.SetProperty(ns, "source", source, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "source");
				}
			}
		}

		/// <summary>
		/// An unordered array of descriptive phrases or keywords that specify the topic of the content of the resource.
		/// </summary>
		public XmpArray<string> Subject
		{
			get { return common.Subject; }
		}

		/// <summary>
		/// The title of the document, or the name given to the resource. 
		/// Typically, it will be a name by which the resource is formally known.
		/// </summary>
		public XmpLangAlt Title
		{
			get { return common.Title; }
		}

		private string type;
		/// <summary>
		/// A document type; for example, novel, poem, or working paper.
		/// </summary>
		public string Type
		{
			get { return type; }
			set
			{
				type = value;
				if (type != null)
				{
					xmpCore.SetProperty(ns, "type", type, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "type");
				}
			}
		}

		protected XmpCore xmpCore;
		private CommonFields.DublinCoreSchema common;
	}
}