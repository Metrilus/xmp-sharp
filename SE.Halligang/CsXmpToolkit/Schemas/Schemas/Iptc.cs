using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	/// <summary>
	/// IPTC Core Schema
	/// </summary>
	public sealed class Iptc
	{
		/// <summary>
		/// Constructs a DublinCore object.
		/// </summary>
		/// <param name="xmp"></param>
		public Iptc(Xmp xmp)
		{
			this.xmpCore = xmp.XmpCore;
			this.dublinCore = xmp.CommonFields.DublinCore;
			this.photoshop = xmp.CommonFields.Photoshop;
			this.xmpRights = xmp.CommonFields.XmpRights;

			PropertyFlags options;
			DateTime dateValue;

			xmpCore.GetProperty(Photoshop.Namespace, "City", out photoshop.City, out options);
			dublinCore.Rights = new XmpLangAlt(xmpCore, DublinCore.Namespace, "rights");
			xmpCore.GetProperty(Photoshop.Namespace, "Country", out photoshop.Country, out options);
			xmpCore.GetProperty(ns, "CountryCode", out countryCode, out options);
			dublinCore.Creator = new XmpArray<string>(xmpCore, DublinCore.Namespace, "creator", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsOrdered, new XmpArrayCallback<string>(XmpArray.StringCallback));
			creatorContactInfo = new ContactInfo(xmpCore, ns, "CreatorContactInfo");
			xmpCore.GetProperty(Photoshop.Namespace, "AuthorsPosition", out photoshop.AuthorsPosition, out options);
			if (xmpCore.GetProperty(Photoshop.Namespace, "DateCreated", out dateValue, out options))
			{
				photoshop.DateCreated = dateValue;
			}
			else
			{
				photoshop.DateCreated = null;
			}
			dublinCore.Description = new XmpLangAlt(xmpCore, DublinCore.Namespace, "description");
			xmpCore.GetProperty(Photoshop.Namespace, "CaptionWriter", out photoshop.CaptionWriter, out options);
			xmpCore.GetProperty(Photoshop.Namespace, "Headline", out photoshop.Headline, out options);
			xmpCore.GetProperty(Photoshop.Namespace, "Instructions", out photoshop.Instructions, out options);
			xmpCore.GetProperty(ns, "IntellectualGenre", out intellectualGenre, out options);
			xmpCore.GetProperty(Photoshop.Namespace, "TransmissionReference", out photoshop.TransmissionReference, out options);
			dublinCore.Subject = new XmpArray<string>(xmpCore, DublinCore.Namespace, "subject", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<string>(XmpArray.StringCallback));
			xmpCore.GetProperty(ns, "Location", out location, out options);
			xmpCore.GetProperty(Photoshop.Namespace, "Credit", out photoshop.Credit, out options);
			xmpCore.GetProperty(Photoshop.Namespace, "State", out photoshop.State, out options);
			xmpRights.UsageTerms = new XmpLangAlt(xmpCore, XmpRights.Namespace, "UsageTerms");
			scene = new XmpArray<string>(xmpCore, ns, "Scene", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<string>(XmpArray.StringCallback));
			xmpCore.GetProperty(Photoshop.Namespace, "Source", out photoshop.Source, out options);
			subjectCode = new XmpArray<string>(xmpCore, ns, "SubjectCode", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<string>(XmpArray.StringCallback));
			dublinCore.Title = new XmpLangAlt(xmpCore, DublinCore.Namespace, "title");
		}

		private static readonly string ns = "http://iptc.org/std/Iptc4xmpCore/1.0/xmlns/";
		/// <summary>
		/// Gets the namespace URI for the struct.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string prefix = "Iptc4xmpCore";
		/// <summary>
		/// Gets the preferred prefix for the schema.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return prefix; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string City
		{
			get { return photoshop.City; }
			set
			{
				photoshop.City = value;
				if (photoshop.City != null)
				{
					xmpCore.SetProperty(Photoshop.Namespace, "City", photoshop.City, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(Photoshop.Namespace, "City");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public XmpLangAlt CopyrightNotice
		{
			get { return dublinCore.Rights; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Country
		{
			get { return photoshop.Country; }
			set
			{
				photoshop.Country = value;
				if (photoshop.Country != null)
				{
					xmpCore.SetProperty(Photoshop.Namespace, "Country", photoshop.Country, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(Photoshop.Namespace, "Country");
				}
			}
		}

		private string countryCode;
		/// <summary>
		/// 
		/// </summary>
		public string CountryCode
		{
			get { return countryCode; }
			set
			{
				countryCode = value;
				if (countryCode != null)
				{
					xmpCore.SetProperty(ns, "CountryCode", countryCode, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "CountryCode");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public XmpArray<string> Creator
		{
			get { return dublinCore.Creator; }
		}

		private ContactInfo creatorContactInfo;
		/// <summary>
		/// 
		/// </summary>
		public ContactInfo CreatorContactInfo
		{
			get { return creatorContactInfo; }
			set { creatorContactInfo = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string CreatorJobtitle
		{
			get { return photoshop.AuthorsPosition; }
			set
			{
				photoshop.AuthorsPosition = value;
				if (photoshop.AuthorsPosition != null)
				{
					xmpCore.SetProperty(Photoshop.Namespace, "AuthorsPosition", photoshop.AuthorsPosition, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(Photoshop.Namespace, "AuthorsPosition");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Nullable<DateTime> DateCreated
		{
			get { return photoshop.DateCreated; }
			set
			{
				photoshop.DateCreated = value;
				if (photoshop.DateCreated != null)
				{
					xmpCore.SetProperty(Photoshop.Namespace, "DateCreated", photoshop.DateCreated.Value, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(Photoshop.Namespace, "DateCreated");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public XmpLangAlt Description
		{
			get { return dublinCore.Description; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string DescriptionWriter
		{
			get { return photoshop.CaptionWriter; }
			set
			{
				photoshop.CaptionWriter = value;
				if (photoshop.CaptionWriter != null)
				{
					xmpCore.SetProperty(Photoshop.Namespace, "CaptionWriter", photoshop.CaptionWriter, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(Photoshop.Namespace, "CaptionWriter");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string Headline
		{
			get { return photoshop.Headline; }
			set
			{
				photoshop.Headline = value;
				if (photoshop.Headline != null)
				{
					xmpCore.SetProperty(Photoshop.Namespace, "Headline", photoshop.Headline, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(Photoshop.Namespace, "Headline");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string Instructions
		{
			get { return photoshop.Instructions; }
			set
			{
				photoshop.Instructions = value;
				if (photoshop.Instructions != null)
				{
					xmpCore.SetProperty(Photoshop.Namespace, "Instructions", photoshop.Instructions, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(Photoshop.Namespace, "Instructions");
				}
			}
		}

		private string intellectualGenre;
		/// <summary>
		/// 
		/// </summary>
		public string IntellectualGenre
		{
			get { return intellectualGenre; }
			set
			{
				intellectualGenre = value;
				if (intellectualGenre != null)
				{
					xmpCore.SetProperty(ns, "IntellectualGenre", intellectualGenre, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "IntellectualGenre");
				}
			}		
		}

		/// <summary>
		/// 
		/// </summary>
		public string JobID
		{
			get { return photoshop.TransmissionReference; }
			set
			{
				photoshop.TransmissionReference = value;
				if (photoshop.TransmissionReference != null)
				{
					xmpCore.SetProperty(Photoshop.Namespace, "TransmissionReference", photoshop.TransmissionReference, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(Photoshop.Namespace, "TransmissionReference");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public XmpArray<string> Keywords
		{
			get { return dublinCore.Subject; }
		}

		private string location;
		/// <summary>
		/// 
		/// </summary>
		public string Location
		{
			get { return location; }
			set
			{
				location = value;
				if (location != null)
				{
					xmpCore.SetProperty(ns, "Location", location, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Location");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string Provider
		{
			get { return photoshop.Credit; }
			set
			{
				photoshop.Credit = value;
				if (photoshop.Credit != null)
				{
					xmpCore.SetProperty(Photoshop.Namespace, "Credit", photoshop.Credit, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(Photoshop.Namespace, "Credit");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string ProvinceOrState
		{
			get { return photoshop.State; }
			set
			{
				photoshop.State = value;
				if (photoshop.State != null)
				{
					xmpCore.SetProperty(Photoshop.Namespace, "State", photoshop.State, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(Photoshop.Namespace, "State");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public XmpLangAlt RightsUsageTerms
		{
			get { return xmpRights.UsageTerms; }
		}

		private XmpArray<string> scene;
		/// <summary>
		/// 
		/// </summary>
		public XmpArray<string> Scene
		{
			get { return scene; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Source
		{
			get { return photoshop.Source; }
			set
			{
				photoshop.Source = value;
				if (photoshop.Source != null)
				{
					xmpCore.SetProperty(Photoshop.Namespace, "Source", photoshop.Source, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(Photoshop.Namespace, "Source");
				}
			}
		}

		private XmpArray<string> subjectCode;
		/// <summary>
		/// 
		/// </summary>
		public XmpArray<string> SubjectCode
		{
			get { return subjectCode; }
		}

		/// <summary>
		/// 
		/// </summary>
		public XmpLangAlt Title
		{
			get { return dublinCore.Title; }
		}

		private XmpCore xmpCore;
		private CommonFields.DublinCoreSchema dublinCore;
		private CommonFields.PhotoshopSchema photoshop;
		private CommonFields.XmpRightsSchema xmpRights;

		internal static string RegisterNamespace()
		{
			string registeredPrefix;
			XmpCore.RegisterNamespace(Iptc.Namespace, Iptc.PreferredPrefix, out registeredPrefix);
			return registeredPrefix;
		}
	}
}
