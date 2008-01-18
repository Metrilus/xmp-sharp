using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	/// <summary>
	/// 
	/// </summary>
	public class Dimensions
	{
		internal Dimensions(XmpCore xmpCore, string schemaNamespace, string structPath)
		{
			this.xmpCore = xmpCore;
			this.schemaNamespace = schemaNamespace;
			this.structPath = structPath;

			PropertyFlags options;
			string tempDouble;

			xmpCore.GetStructField(schemaNamespace, structPath, ns, "w", out tempDouble, out options);
			if (tempDouble == null)
			{
				width = null;
			}
			else
			{
				width = XmpUtils.ConvertToFloat(tempDouble);
			}
			xmpCore.GetStructField(schemaNamespace, structPath, ns, "h", out tempDouble, out options);
			if (tempDouble == null)
			{
				height = null;
			}
			else
			{
				height = XmpUtils.ConvertToFloat(tempDouble);
			}
			xmpCore.GetStructField(schemaNamespace, structPath, ns, "unit", out unit, out options);
		}

		private static readonly string ns = "http://ns.adobe.com/xap/1.0/sType/Dimensions#";
		/// <summary>
		/// Gets the namespace URI for the struct.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string prefix = "stDim";
		/// <summary>
		/// Gets the preferred prefix for the struct.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return prefix; }
		}

		private Nullable<double> width;
		/// <summary>
		/// Width
		/// </summary>
		public Nullable<double> Width
		{
			get	{ return width; }
			set
			{
				width = value;
				if (width.HasValue)
				{
					string widthString;
					XmpUtils.ConvertFromFloat(width.Value, null, out widthString);
					xmpCore.SetStructField(schemaNamespace, structPath, ns, "w", widthString, PropertyFlags.None);
				}
				else if (HasValue())
				{
					xmpCore.DeleteStructField(schemaNamespace, structPath, ns, "w");
				}
				else
				{
					xmpCore.DeleteProperty(schemaNamespace, structPath);
				}
			}
		}

		private Nullable<double> height;
		/// <summary>
		/// Height
		/// </summary>
		public Nullable<double> Height
		{
			get	{ return height; }
			set
			{
				height = value;
				if (height.HasValue)
				{
					string heightString;
					XmpUtils.ConvertFromFloat(height.Value, null, out heightString);
					xmpCore.SetStructField(schemaNamespace, structPath, ns, "h", heightString, PropertyFlags.None);
				}
				else if (HasValue())
				{
					xmpCore.DeleteStructField(schemaNamespace, structPath, ns, "h");
				}
				else
				{
					xmpCore.DeleteProperty(schemaNamespace, structPath);
				}
			}
		}

		private string unit;
		/// <summary>
		/// Units. For example: inch, mm, pixel, pica, point
		/// </summary>
		public string Unit
		{
			get { return unit; }
			set
			{
				unit = value;
				if (unit != null)
				{
					xmpCore.SetStructField(schemaNamespace, structPath, ns, "unit", unit, PropertyFlags.None);
				}
				else if (HasValue())
				{
					xmpCore.DeleteStructField(schemaNamespace, structPath, ns, "unit");
				}
				else
				{
					xmpCore.DeleteProperty(schemaNamespace, structPath);
				}
			}
		}

		private XmpCore xmpCore;
		private string schemaNamespace;
		private string structPath;

		public void SetDimensions(double width, double height, string unit)
		{
			Width = width;
			Height = height;
			Unit = unit;
		}

		public void Clear()
		{
			xmpCore.DeleteProperty(schemaNamespace, structPath);
		}

		internal static string RegisterNamespace()
		{
			string registeredPrefix;
			XmpCore.RegisterNamespace(Dimensions.Namespace, Dimensions.PreferredPrefix, out registeredPrefix);
			return registeredPrefix;
		}

		private bool HasValue()
		{
			return (width.HasValue || height.HasValue || unit != null);
		}
	}
}