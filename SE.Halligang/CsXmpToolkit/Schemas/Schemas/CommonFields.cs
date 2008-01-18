using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	internal sealed class CommonFields
	{
		private DublinCoreSchema dublinCore = new DublinCoreSchema();
		/// <summary>
		/// 
		/// </summary>
		public DublinCoreSchema DublinCore
		{
			get { return dublinCore; }
		}
	
		private PhotoshopSchema photoshop = new PhotoshopSchema();
		/// <summary>
		/// 
		/// </summary>
		public PhotoshopSchema Photoshop
		{
			get { return photoshop; }
		}

		private XmpRightsSchema xmpRights = new XmpRightsSchema();
		/// <summary>
		/// 
		/// </summary>
		public XmpRightsSchema XmpRights
		{
			get { return xmpRights; }
			set { xmpRights = value; }
		}	

		public class DublinCoreSchema
		{
			public XmpArray<string> Creator;
			public XmpLangAlt Description;
			public XmpLangAlt Rights;
			public XmpArray<string> Subject;
			public XmpLangAlt Title;
		}

		public class PhotoshopSchema
		{
			public string AuthorsPosition;
			public string CaptionWriter;
			public string City;
			public string Country;
			public string Credit;
			public Nullable<DateTime> DateCreated;
			public string Headline;
			public string Instructions;
			public string Source;
			public string State;
			public string TransmissionReference;
		}

		public class XmpRightsSchema
		{
			public XmpLangAlt UsageTerms;
		}
	}
}
