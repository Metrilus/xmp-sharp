using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	public class Area
	{
		public Area(ShapeType type, int[] cords, LangEntry[] title, LangEntry[] description, string target)
		{
			this.type = type;
			this.cords = cords;
			if (title != null)
			{
				this.title = title;
			}
			else
			{
				this.title = new LangEntry[] { };
			}
			if (description != null)
			{
				this.description = description;
			}
			else
			{
				this.description = new LangEntry[] { };
			}
			this.target = target;

			imageMap = null;
		}

		internal Area(ShapeType type, int[] cords, LangEntry[] title, LangEntry[] description, string target, ImageMap imageMap)
		{
			this.type = type;
			this.cords = cords;
			this.title = title;
			this.description = description;
			this.target = target;

			this.imageMap = imageMap;
		}

		private static readonly string ns = "http://ns.halligang.se/imagemap/1.0/area/";
		/// <summary>
		/// Gets the namespace URI for the struct.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string prefix = "imArea";
		/// <summary>
		/// Gets the preferred prefix for the struct.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return prefix; }
		}

		private ImageMap imageMap;
		/// <summary>
		/// 
		/// </summary>
		internal ImageMap ImageMap
		{
			get { return imageMap; }
			set { imageMap = value; }
		}	

		private ShapeType type;
		/// <summary>
		/// 
		/// </summary>
		public ShapeType Type
		{
			get { return type; }
		}

		private int[] cords;
		/// <summary>
		///
		/// </summary>
		public int[] Cords
		{
			get { return cords; }
		}

		private LangEntry[] title;
		/// <summary>
		///
		/// </summary>
		public LangEntry[] Title
		{
			get { return title; }
		}

		private LangEntry[] description;
		/// <summary>
		///
		/// </summary>
		public LangEntry[] Description
		{
			get { return description; }
		}

		private string target;
		/// <summary>
		/// 
		/// </summary>
		public string Target
		{
			get { return target; }
		}

		internal static string RegisterNamespace()
		{
			string registeredPrefix;
			XmpCore.RegisterNamespace(Area.Namespace, Area.PreferredPrefix, out registeredPrefix);
			return registeredPrefix;
		}
	}
}