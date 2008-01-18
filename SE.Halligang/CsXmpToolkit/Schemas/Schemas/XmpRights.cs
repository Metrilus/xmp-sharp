using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	/// <summary>
	/// XMP Rights Management Schema
	/// </summary>
	public sealed class XmpRights
	{
		/// <summary>
		/// Constructs an XmpRights object.
		/// </summary>
		/// <param name="xmp"></param>
		public XmpRights(Xmp xmp)
		{
			this.xmpCore = xmp.XmpCore;
			this.common = xmp.CommonFields.XmpRights;

			PropertyFlags options;
			bool boolValue;

			xmpCore.GetProperty(ns, "Certificate", out certificate, out options);
			if (xmpCore.GetProperty(ns, "Marked", out boolValue, out options))
			{
				marked = boolValue;
			}
			owner = new XmpArray<string>(xmpCore, ns, "Owner", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<string>(XmpArray.StringCallback));
			common.UsageTerms = new XmpLangAlt(xmpCore, ns, "UsageTerms");
			xmpCore.GetProperty(ns, "WebStatement", out webStatement, out options);
		}

		private static readonly string ns = "http://ns.adobe.com/xap/1.0/rights/";
		/// <summary>
		/// Gets the namespace URI for the schema.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string preferredPrefix = "xmpRights";
		/// <summary>
		/// Gets the preferred prefix for the schema.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return preferredPrefix; }
		}

		private string certificate;
		/// <summary>
		/// Online rights management certificate.
		/// </summary>
		public string Certificate
		{
			get { return certificate; }
			set
			{
				certificate = value;
				if (certificate != null)
				{
					xmpCore.SetProperty(ns, "Certificate", certificate, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Certificate");
				}
			}
		}

		private Nullable<bool> marked;
		/// <summary>
		/// Indicates that this is a rights-managed resource.
		/// </summary>
		public Nullable<bool> Marked
		{
			get { return marked; }
			set
			{
				marked = value;
				if (marked != null)
				{
					xmpCore.SetProperty(ns, "Marked", marked.Value, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Marked");
				}
			}
		}

		private XmpArray<string> owner;
		/// <summary>
		/// An unordered array specifying the legal owner(s)
		/// of a resource.
		/// </summary>
		public XmpArray<string> Owner
		{
			get { return owner; }
		}

		/// <summary>
		/// Text instructions on how a resource can be
		/// legally used.
		/// </summary>
		public XmpLangAlt UsageTerms
		{
			get { return common.UsageTerms; }
		}

		private string webStatement = null;
		/// <summary>
		/// The location of a web page describing the owner
		/// and/or rights statement for this resource.
		/// </summary>
		public string WebStatement
		{
			get { return webStatement; }
			set
			{
				webStatement = value;
				if (webStatement != null)
				{
					xmpCore.SetProperty(ns, "WebStatement", webStatement, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "WebStatement");
				}
			}
		}

		private XmpCore xmpCore;
		private CommonFields.XmpRightsSchema common;
	}
}
