using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	/// <summary>
	/// Photoshop Schema
	/// </summary>
	public sealed class Photoshop
	{
		/// <summary>
		/// Constructs a Photoshop object.
		/// </summary>
		/// <param name="xmp"></param>
		public Photoshop(Xmp xmp)
		{
			this.xmpCore = xmp.XmpCore;
			this.common = xmp.CommonFields.Photoshop;

			PropertyFlags options;
			DateTime dateValue;
			int intValue;

			xmpCore.GetProperty(ns, "AuthorsPosition", out common.AuthorsPosition, out options);
			xmpCore.GetProperty(ns, "CaptionWriter", out common.CaptionWriter, out options);
			xmpCore.GetProperty(ns, "Category", out category, out options);
			xmpCore.GetProperty(ns, "City", out common.City, out options);
			xmpCore.GetProperty(ns, "Country", out common.Country, out options);
			xmpCore.GetProperty(ns, "Credit", out common.Credit, out options);
			if (xmpCore.GetProperty(ns, "DateCreated", out dateValue, out options))
			{
				common.DateCreated = dateValue;
			}
			xmpCore.GetProperty(ns, "Headline", out common.Headline, out options);
			xmpCore.GetProperty(ns, "Instructions", out common.Instructions, out options);
			xmpCore.GetProperty(ns, "Source", out common.Source, out options);
			xmpCore.GetProperty(ns, "State", out common.State, out options);
			supplementalCategories = new XmpArray<string>(xmpCore, ns, "SupplementalCategories", PropertyFlags.ValueIsArray | PropertyFlags.ArrayIsUnordered, new XmpArrayCallback<string>(XmpArray.StringCallback));
			xmpCore.GetProperty(ns, "TransmissionReference", out common.TransmissionReference, out options);
			if (xmpCore.GetProperty(ns, "Urgency", out intValue, out options))
			{
				urgency = intValue;
			}
		}

		private static readonly string ns = "http://ns.adobe.com/photoshop/1.0/";
		/// <summary>
		/// Gets the namespace URI for the schema.
		/// </summary>
		public static string Namespace
		{
			get { return ns; }
		}

		private static readonly string preferredPrefix = "photoshop";
		/// <summary>
		/// Gets the preferred prefix for the schema.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return preferredPrefix; }
		}

		/// <summary>
		/// By-line title.
		/// </summary>
		public string AuthorsPosition
		{
			get { return common.AuthorsPosition; }
			set
			{
				common.AuthorsPosition = value;
				if (common.AuthorsPosition != null)
				{
					xmpCore.SetProperty(ns, "AuthorsPosition", common.AuthorsPosition, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "AuthorsPosition");
				}
			}
		}

		/// <summary>
		/// Writer/editor.
		/// </summary>
		public string CaptionWriter
		{
			get { return common.CaptionWriter; }
			set
			{
				common.CaptionWriter = value;
				if (common.CaptionWriter != null)
				{
					xmpCore.SetProperty(ns, "CaptionWriter", common.CaptionWriter, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "CaptionWriter");
				}
			}
		}

		private string category;
		/// <summary>
		/// Category. Limited to 3 7-bit ASCII characters.
		/// </summary>
		public string Category
		{
			get { return category; }
			set
			{
				category = value;
				if (category != null)
				{
					xmpCore.SetProperty(ns, "Category", category, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Category");
				}
			}
		}

		/// <summary>
		/// City.
		/// </summary>
		public string City
		{
			get { return common.City; }
			set
			{
				common.City = value;
				if (common.City != null)
				{
					xmpCore.SetProperty(ns, "City", common.City, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "City");
				}
			}
		}

		/// <summary>
		/// Country/primary location.
		/// </summary>
		public string Country
		{
			get { return common.Country; }
			set
			{
				common.Country = value;
				if (common.Country != null)
				{
					xmpCore.SetProperty(ns, "Country", common.Country, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Country");
				}
			}
		}

		/// <summary>
		/// Credit.
		/// </summary>
		public string Credit
		{
			get { return common.Credit; }
			set
			{
				common.Credit = value;
				if (common.Credit != null)
				{
					xmpCore.SetProperty(ns, "Credit", common.Credit, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Credit");
				}
			}
		}

		/// <summary>
		/// The date the intellectual content of the document
		/// was created (rather than the creation date of the
		/// physical representation), following IIM conventions.
		/// For example, a photo taken during the American
		/// Civil War would have a creation date during that
		/// epoch (1861-1865) rather than the date the photo
		/// was digitized for archiving.
		/// </summary>
		public Nullable<DateTime> DateCreated
		{
			get { return common.DateCreated; }
			set
			{
				common.DateCreated = value;
				if (common.DateCreated != null)
				{
					xmpCore.SetProperty(ns, "DateCreated", common.DateCreated.Value, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "DateCreated");
				}
			}
		}

		/// <summary>
		/// Headline.
		/// </summary>
		public string Headline
		{
			get { return common.Headline; }
			set
			{
				common.Headline = value;
				if (common.Headline != null)
				{
					xmpCore.SetProperty(ns, "Headline", common.Headline, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Headline");
				}
			}
		}

		/// <summary>
		/// Special instructions.
		/// </summary>
		public string Instructions
		{
			get { return common.Instructions; }
			set
			{
				common.Instructions = value;
				if (common.Instructions != null)
				{
					xmpCore.SetProperty(ns, "Instructions", common.Instructions, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Instructions");
				}
			}
		}

		/// <summary>
		/// Source.
		/// </summary>
		public string Source
		{
			get { return common.Source; }
			set
			{
				common.Source = value;
				if (common.Source != null)
				{
					xmpCore.SetProperty(ns, "Source", common.Source, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Source");
				}
			}
		}

		/// <summary>
		/// Province/state.
		/// </summary>
		public string State
		{
			get { return common.State; }
			set
			{
				common.State = value;
				if (common.State != null)
				{
					xmpCore.SetProperty(ns, "State", common.State, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "State");
				}
			}
		}

		private XmpArray<string> supplementalCategories;
		/// <summary>
		/// Supplemental category.
		/// </summary>
		public XmpArray<string> SupplementalCategories
		{
			get { return supplementalCategories; }
		}

		/// <summary>
		/// Original transmission reference.
		/// </summary>
		public string TransmissionReference
		{
			get { return common.TransmissionReference; }
			set
			{
				common.TransmissionReference = value;
				if (common.TransmissionReference != null)
				{
					xmpCore.SetProperty(ns, "TransmissionReference", common.TransmissionReference, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "TransmissionReference");
				}				
			}
		}

		private Nullable<int> urgency;
		/// <summary>
		/// Urgency. Valid range is 1-8.
		/// </summary>
		public Nullable<int> Urgency
		{
			get { return urgency; }
			set
			{
				if (value.HasValue && (value.Value < 1 || value.Value > 8))
				{
					throw new IndexOutOfRangeException("Valid range is 1-8.");
				}
				urgency = value;
				if (urgency.HasValue)
				{
					xmpCore.SetProperty(ns, "Urgency", urgency.Value, PropertyFlags.None);
				}
				else
				{
					xmpCore.DeleteProperty(ns, "Urgency");
				}
			}
		}

		private XmpCore xmpCore;
		private CommonFields.PhotoshopSchema common;
	}
}
