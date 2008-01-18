using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	public class ContactInfo
	{
		internal ContactInfo(XmpCore xmpCore, string schemaNamespace, string structPath)
		{
			this.xmpCore = xmpCore;
			this.schemaNamespace = schemaNamespace;
			this.structPath = structPath;

			PropertyFlags options;

			xmpCore.GetStructField(schemaNamespace, structPath, ns, "CiAdrCity", out ciAdrCity, out options);
			xmpCore.GetStructField(schemaNamespace, structPath, ns, "CiAdrCtry", out ciAdrCtry, out options);
			xmpCore.GetStructField(schemaNamespace, structPath, ns, "CiAdrExtadr", out ciAdrExtadr, out options);
			xmpCore.GetStructField(schemaNamespace, structPath, ns, "CiAdrPcode", out ciAdrPcode, out options);
			xmpCore.GetStructField(schemaNamespace, structPath, ns, "CiAdrRegion", out ciAdrRegion, out options);
			xmpCore.GetStructField(schemaNamespace, structPath, ns, "CiEmailWork", out ciEmailWork, out options);
			xmpCore.GetStructField(schemaNamespace, structPath, ns, "CiTelWork", out ciTelWork, out options);
			xmpCore.GetStructField(schemaNamespace, structPath, ns, "CiUrlWork", out ciUrlWork, out options);
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
		/// Gets the preferred prefix for the struct.
		/// </summary>
		internal static string PreferredPrefix
		{
			get { return prefix; }
		}

		private string ciAdrCity;
		/// <summary>
		/// The contact information city part.
		/// </summary>
		//public string CiAdrCity
		public string City
		{
			get { return ciAdrCity; }
			set
			{
				ciAdrCity = value;
				if (ciAdrCity != null)
				{
					xmpCore.SetStructField(schemaNamespace, structPath, ns, "CiAdrCity", ciAdrCity, PropertyFlags.None);
				}
				else if (HasValue())
				{
					xmpCore.DeleteStructField(schemaNamespace, structPath, ns, "CiAdrCity");
				}
				else
				{
					xmpCore.DeleteProperty(schemaNamespace, structPath);
				}
			}
		}

		private string ciAdrCtry;
		/// <summary>
		/// The contact information county part.
		/// </summary>
		//public string CiAddrCtry
		public string Country
		{
			get { return ciAdrCtry; }
			set
			{
				ciAdrCtry = value;
				if (ciAdrCtry != null)
				{
					xmpCore.SetStructField(schemaNamespace, structPath, ns, "CiAdrCtry", ciAdrCtry, PropertyFlags.None);
				}
				else if (HasValue())
				{
					xmpCore.DeleteStructField(schemaNamespace, structPath, ns, "CiAdrCtry");
				}
				else
				{
					xmpCore.DeleteProperty(schemaNamespace, structPath);
				}
			}
		}

		private string ciAdrExtadr;
		/// <summary>
		/// The contact information address part. Comprises an optional company name and all
		/// required information to locate the building or postbox to which mail should be sent. To that
		/// end, the address is a multiline field.
		/// </summary>
		//public string CiAdrExtadr
		public string Address
		{
			get { return ciAdrExtadr; }
			set
			{
				ciAdrExtadr = value;
				if (ciAdrExtadr != null)
				{
					xmpCore.SetStructField(schemaNamespace, structPath, ns, "CiAdrExtadr", ciAdrExtadr, PropertyFlags.None);
				}
				else if (HasValue())
				{
					xmpCore.DeleteStructField(schemaNamespace, structPath, ns, "CiAdrExtadr");
				}
				else
				{
					xmpCore.DeleteProperty(schemaNamespace, structPath);
				}
			}
		}

		private string ciAdrPcode;
		/// <summary>
		/// The contact information part denoting the local postal code.
		/// </summary>
		//public string CiAdrPcode
		public string PostalCode
		{
			get { return ciAdrPcode; }
			set
			{
				ciAdrPcode = value;
				if (ciAdrPcode != null)
				{
					xmpCore.SetStructField(schemaNamespace, structPath, ns, "CiAdrPcode", ciAdrPcode, PropertyFlags.None);
				}
				else if (HasValue())
				{
					xmpCore.DeleteStructField(schemaNamespace, structPath, ns, "CiAdrPcode");
				}
				else
				{
					xmpCore.DeleteProperty(schemaNamespace, structPath);
				}
			}
		}

		private string ciAdrRegion;
		/// <summary>
		/// The contact information part denoting regional information like state or province.
		/// </summary>
		//public string CiAdrRegion
		public string StateOrProvince
		{
			get { return ciAdrRegion; }
			set
			{
				ciAdrRegion = value;
				if (ciAdrRegion != null)
				{
					xmpCore.SetStructField(schemaNamespace, structPath, ns, "CiAdrRegion", ciAdrRegion, PropertyFlags.None);
				}
				else if (HasValue())
				{
					xmpCore.DeleteStructField(schemaNamespace, structPath, ns, "CiAdrRegion");
				}
				else
				{
					xmpCore.DeleteProperty(schemaNamespace, structPath);
				}
			}
		}

		private string ciEmailWork;
		/// <summary>
		/// The contact information email address part. Multiple email addresses can be given,
		/// separated by a comma.
		/// </summary>
		//public string CiEmailWork
		public string EMail
		{
			get { return ciEmailWork; }
			set
			{
				ciEmailWork = value;
				if (ciEmailWork != null)
				{
					xmpCore.SetStructField(schemaNamespace, structPath, ns, "CiEmailWork", ciEmailWork, PropertyFlags.None);
				}
				else if (HasValue())
				{
					xmpCore.DeleteStructField(schemaNamespace, structPath, ns, "CiEmailWork");
				}
				else
				{
					xmpCore.DeleteProperty(schemaNamespace, structPath);
				}
			}
		}

		private string ciTelWork;
		/// <summary>
		/// The contact information phone number part. Multiple numbers can be given, separated by
		/// a comma.
		/// </summary>
		//public string CiTelWork
		public string Phone
		{
			get { return ciTelWork; }
			set
			{
				ciTelWork = value;
				if (ciTelWork != null)
				{
					xmpCore.SetStructField(schemaNamespace, structPath, ns, "CiTelWork", ciTelWork, PropertyFlags.None);
				}
				else if (HasValue())
				{
					xmpCore.DeleteStructField(schemaNamespace, structPath, ns, "CiTelWork");
				}
				else
				{
					xmpCore.DeleteProperty(schemaNamespace, structPath);
				}
			}
		}

		private string ciUrlWork;
		/// <summary>
		/// The contact information web address part. Multiple addresses can be given, separated by
		/// a comma.
		/// </summary>
		//public string CiUrlWork
		public string WebUrl
		{
			get { return ciUrlWork; }
			set
			{
				ciUrlWork = value;
				if (ciUrlWork != null)
				{
					xmpCore.SetStructField(schemaNamespace, structPath, ns, "CiUrlWork", ciUrlWork, PropertyFlags.None);
				}
				else if (HasValue())
				{
					xmpCore.DeleteStructField(schemaNamespace, structPath, ns, "CiUrlWork");
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

		public void SetContactInfo(string city, string country, string address, string postalCode,
			string stateOrProvince, string eMail, string phone, string webUrl)
		{
			City = city;
			Country = country;
			Address = address;
			PostalCode = postalCode;
			StateOrProvince = stateOrProvince;
			EMail = eMail;
			Phone = phone;
			WebUrl = webUrl;
		}

		public void Clear()
		{
			xmpCore.DeleteProperty(schemaNamespace, structPath);
		}

		internal static string RegisterNamespace()
		{
			string registeredPrefix;
			XmpCore.RegisterNamespace(ContactInfo.Namespace, ContactInfo.PreferredPrefix, out registeredPrefix);
			return registeredPrefix;
		}

		private bool HasValue()
		{
			return (ciAdrCity != null ||
				ciAdrCtry != null ||
				ciAdrExtadr != null ||
				ciAdrPcode != null ||
				ciAdrRegion != null ||
				ciEmailWork != null ||
				ciTelWork != null ||
				ciUrlWork != null);
		}
	}
}
