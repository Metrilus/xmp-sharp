using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// 
	/// </summary>
	public class XmpVersionInfo
	{
		internal XmpVersionInfo(PInvoke.XmpVersionInfo versionInfo)
		{
			this.major = versionInfo.Major;
			this.minor = versionInfo.Minor;
			this.micro = versionInfo.Micro;
			this.isDebug = (versionInfo.IsDebug != 0);
			this.build = versionInfo.Build;
			this.flags = versionInfo.Flags;
			this.message = MarshalHelper.GetString(versionInfo.Message, 0, 256, Encoding.UTF8);
		}

		private int major;
		/// <summary>
		/// The primary release number, the "1" in version "1.2.3".
		/// </summary>
		public int Major
		{
			get { return major; }
		}

		private int minor;
		/// <summary>
		/// The secondary release number, the "2" in version "1.2.3".
		/// </summary>
		public int Minor
		{
			get { return minor; }
		}

		private int micro;
		/// <summary>
		/// The tertiary release number, the "3" in version "1.2.3".
		/// </summary>
		public int Micro
		{
			get { return micro; }
		}

		private bool isDebug;
		/// <summary>
		/// Really a 0/1 bool value. True if this is a debug build.
		/// </summary>
		public bool IsDebug
		{
			get { return isDebug; }
		}

		private int build;
		/// <summary>
		/// A rolling build number, monotonically increasing in a release.
		/// </summary>
		public int Build
		{
			get { return build; }
		}

		private int flags;
		/// <summary>
		/// Individual feature implementation flags.
		/// </summary>
		public int Flags
		{
			get { return flags; }
		}
	
		private string message;
		/// <summary>
		/// A comprehensive version information string.
		/// </summary>
		public string Message
		{
			get { return message; }
		}	
	}
}
