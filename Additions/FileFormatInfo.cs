using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// A helper class that substitutes XmpFiles.GetFormatInfo.
	/// </summary>
	public class FileFormatInfo
	{
		/// <summary>
		/// Retrieves file format information for the given file format.
		/// </summary>
		/// <param name="fileFormat">File format to get information for.</param>
		public FileFormatInfo(FileFormat fileFormat)
		{
			this.fileFormat = fileFormat;
			if (XmpFiles.GetFormatInfo(fileFormat, out formatInfo))
			{
				this.scanningMode = ScanningMode.Smart;
			}
			else
			{
				this.scanningMode = ScanningMode.Packet;
			}
		}

		private FileFormat fileFormat;
		/// <summary>
		/// 
		/// </summary>
		public FileFormat FileFormat
		{
			get { return fileFormat; }
		}

		private FormatInfo formatInfo;
		/// <summary>
		/// 
		/// </summary>
		public FormatInfo FormatInfo
		{
			get { return formatInfo; }
		}

		private ScanningMode scanningMode;
		/// <summary>
		/// 
		/// </summary>
		public ScanningMode ScanningMode
		{
			get { return scanningMode; }
		}	
	}
}
