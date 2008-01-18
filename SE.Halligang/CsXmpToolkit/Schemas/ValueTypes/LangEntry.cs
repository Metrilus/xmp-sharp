using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	public class LangEntry
	{
		public LangEntry(string language, string value)
		{
			this.language = language;
			this.value = value;
		}

		private string language;
		/// <summary>
		/// 
		/// </summary>
		public string Language
		{
			get { return language; }
		}

		private string value;
		/// <summary>
		/// 
		/// </summary>
		public string Value
		{
			get { return value; }
		}	
	}
}
