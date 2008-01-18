using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	public class Job
	{
		public Job(string name, string id, string url)
		{
			this.name = name;
			this.id = id;
			this.url = url;
		}

		private static readonly string ns = "http://ns.adobe.com/xap/1.0/sType/Job#";
		/// <summary>
		/// Gets the namespace URI for the struct.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string prefix = "stJob";
		/// <summary>
		/// Gets the preferred prefix for the struct.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return prefix; }
		}

		private string name;
		/// <summary>
		/// 
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		private string id;
		/// <summary>
		/// 
		/// </summary>
		public string ID
		{
			get { return id; }
		}

		private string url;
		/// <summary>
		/// 
		/// </summary>
		public string Url
		{
			get { return url; }
		}

		internal static string RegisterNamespace()
		{
			string registeredPrefix;
			XmpCore.RegisterNamespace(Job.Namespace, Job.PreferredPrefix, out registeredPrefix);
			return registeredPrefix;
		}
	}
}
