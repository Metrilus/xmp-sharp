using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	/// <summary>
	/// Adobe PDF Schema
	/// </summary>
	public sealed class Pdf
	{
		/// <summary>
		/// Constructs a Pdf object.
		/// </summary>
		/// <param name="xmp"></param>
		public Pdf(Xmp xmp)
		{
			this.xmpCore = xmp.XmpCore;

			RegisterNamespace();

			PropertyFlags options;

			xmpCore.GetProperty(ns, "Keywords", out keywords, out options);
			xmpCore.GetProperty(ns, "PDFVersion", out pdfVersion, out options);
			xmpCore.GetProperty(ns, "Producer", out producer, out options);
		}

		private static readonly string ns = "http://ns.adobe.com/pdf/1.3/";
		/// <summary>
		/// Gets the namespace URI for the schema.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string preferredPrefix = "pdf";
		/// <summary>
		/// Gets the preferred prefix for the schema.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return preferredPrefix; }
		}

		private string keywords;
		/// <summary>
		/// 
		/// </summary>
		public string Keywords
		{
			get { return keywords; }
			set
			{
				keywords = value;
				if (keywords != null)
				{
					xmpCore.SetProperty(ns, "Keywords", keywords, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Keywords");
				}				
			}
		}

		private string pdfVersion;
		/// <summary>
		/// The extent or scope of the resource.
		/// </summary>
		public string PdfVersion
		{
			get { return pdfVersion; }
			set
			{
				pdfVersion = value;
				if (pdfVersion != null)
				{
					xmpCore.SetProperty(ns, "PDFVersion", pdfVersion, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "PDFVersion");
				}
			}
		}

		private string producer;
		/// <summary>
		/// The extent or scope of the resource.
		/// </summary>
		public string Producer
		{
			get { return producer; }
			set
			{
				producer = value;
				if (producer != null)
				{
					xmpCore.SetProperty(ns, "Producer", producer, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Producer");
				}
			}
		}

		private XmpCore xmpCore;

		internal static string RegisterNamespace()
		{
			string registeredPrefix;
			XmpCore.RegisterNamespace(Pdf.Namespace, Pdf.PreferredPrefix, out registeredPrefix);
			return registeredPrefix;
		}
	}
}