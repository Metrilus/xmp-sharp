using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	/// <summary>
	/// EXIF Schema for Additional EXIF Properties
	/// </summary>
	public sealed class ExifAdditional
	{
		/// <summary>
		/// Constructs an ExifAdditional object.
		/// </summary>
		/// <param name="xmp"></param>
		public ExifAdditional(Xmp xmp)
		{
			this.xmpCore = xmp.XmpCore;

			RegisterNamespace();

			PropertyFlags options;

			xmpCore.GetProperty(ns, "Lens", out lens, out options);
			xmpCore.GetProperty(ns, "SerialNumber", out serialNumber, out options);
		}

		private static readonly string ns = "http://ns.adobe.com/exif/1.0/aux/";
		/// <summary>
		/// Gets the namespace URI for the schema.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string preferredPrefix = "aux";
		/// <summary>
		/// Gets the preferred prefix for the schema.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return preferredPrefix; }
		}

		private string lens;
		/// <summary>
		/// 
		/// </summary>
		public string Lens
		{
			get { return lens; }
			set
			{
				lens = value;
				if (lens != null)
				{
					xmpCore.SetProperty(ns, "Lens", lens, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Lens");
				}				
			}
		}

		private string serialNumber;
		/// <summary>
		/// The extent or scope of the resource.
		/// </summary>
		public string SerialNumber
		{
			get { return serialNumber; }
			set
			{
				serialNumber = value;
				if (serialNumber != null)
				{
					xmpCore.SetProperty(ns, "SerialNumber", serialNumber, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "SerialNumber");
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