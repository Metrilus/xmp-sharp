using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	/// <summary>
	/// 
	/// </summary>
	public class Thumbnail
	{
		public Thumbnail(int width, int height, string image)
		{
			this.width = width;
			this.height = height;
			this.image = image;
		}

		private static readonly string ns = "http://ns.adobe.com/xap/1.0/g/img/";
		/// <summary>
		/// Gets the namespace URI for the struct.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string prefix = "xapGImg";
		/// <summary>
		/// Gets the preferred prefix for the struct.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return prefix; }
		}

		private int width;
		/// <summary>
		/// Width in pixels
		/// </summary>
		public int Width
		{
			get { return width; }
		}

		private int height;
		/// <summary>
		/// Height in pixels
		/// </summary>
		public int Height
		{
			get { return height; }
		}

		private string format = "JPEG";
		/// <summary>
		/// The image encoding. Defined value: JPEG.
		/// </summary>
		public string Format
		{
			get { return format; }
		}

		private string image;
		/// <summary>
		/// The thumbnail image (pixel data only) converted to base 64
		/// notation (according to section 6.8 of RFC 2045).
		/// </summary>
		public string Image
		{
			get { return image; }
		}

		internal static string RegisterNamespace()
		{
			string registeredPrefix;
			XmpCore.RegisterNamespace(Thumbnail.Namespace, Thumbnail.PreferredPrefix, out registeredPrefix);
			return registeredPrefix;
		}
	}
}
