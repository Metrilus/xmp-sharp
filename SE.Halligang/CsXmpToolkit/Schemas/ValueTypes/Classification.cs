	using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	public class Classification
	{
		public Classification(string subject, ClassificationType type, string[] path)
		{
			this.subject = subject;
			this.type = type;
			this.path = path;
		}

		private static readonly string ns = "http://ns.halligang.se/pcs/1.0/c/";
		/// <summary>
		/// Gets the namespace URI for the struct.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string prefix = "pcsc";
		/// <summary>
		/// Gets the preferred prefix for the struct.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return prefix; }
		}

		private string	subject;
		/// <summary>
		/// 
		/// </summary>
		public string Subject
		{
			get { return subject; }
		}

		private ClassificationType type;
		/// <summary>
		/// 
		/// </summary>
		public ClassificationType Type
		{
			get { return type; }
		}

		private string[] path;
		/// <summary>
		/// 
		/// </summary>
		public string[] Path
		{
			get { return path; }
		}

		internal static string RegisterNamespace()
		{
			string registeredPrefix;
			XmpCore.RegisterNamespace(Classification.Namespace, Classification.prefix, out registeredPrefix);
			return registeredPrefix;
		}
	}
}
