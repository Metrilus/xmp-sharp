using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	public class Xmp : IDisposable
	{
		private Xmp(string filePath, XmpFileMode fileMode)
		{
			this.filePath = filePath;
			this.fileMode = fileMode;

			lock (lockObject)
			{
				if (initializeCount == 0 && (!XmpCore.Initialize() || !XmpFiles.Initialize()))
				{
					throw new ApplicationException("Failed to initialize XmpCore and/or XmpFiles.");
				}
				initializeCount++;
			}

			xmpFiles = new XmpFiles();

			OpenFlags smartScanningFlags = OpenFlags.OpenUseSmartHandler | OpenFlags.OpenCacheTNail;
			OpenFlags packetScanningFlags = OpenFlags.OpenUsePacketScanning;
			if (fileMode == XmpFileMode.ReadOnly)
			{
				smartScanningFlags |= OpenFlags.OpenForRead;
				packetScanningFlags |= OpenFlags.OpenForRead;
			}
			else
			{
				smartScanningFlags |= OpenFlags.OpenForUpdate;
				packetScanningFlags |= OpenFlags.OpenForUpdate;
			}

			bool smartHandler = true;
			if (!xmpFiles.OpenFile(filePath, FileFormat.Unknown, smartScanningFlags))
			{
				smartHandler = false;
				if (!xmpFiles.OpenFile(filePath, FileFormat.Unknown, packetScanningFlags))
				{
					throw new IOException("Failed to open file.");
				}
			}

			xmpCore = new XmpCore();
			bool xmpExists = xmpFiles.GetXmp(xmpCore);

			// Work-around for AVI files.
			OpenFlags openFlags;
			FileFormat fileFormat;
			FormatInfo formatInfo;
			if (xmpFiles.GetFileInfo(out openFlags, out fileFormat, out formatInfo) &&
				fileFormat == FileFormat.Avi)
			{
				// Reading XMP using smart-handler sometimes fails.
				// Resort to packet scanning.
				if (!xmpExists && smartHandler && fileMode != XmpFileMode.ReadOnly)
				{
					xmpFiles.CloseFile(CloseFlags.None);
					xmpFiles.OpenFile(filePath, FileFormat.Unknown, packetScanningFlags);
					xmpExists = xmpFiles.GetXmp(xmpCore);

					if (xmpExists)
					{
						// XMP was found using packet scanner, keep packet handler
						// and open the file as read-only, just to be safe.
						this.fileMode = XmpFileMode.ReadOnly;
						xmpFiles.CloseFile(CloseFlags.None);
						xmpFiles.OpenFile(filePath, FileFormat.Unknown, OpenFlags.OpenForRead | OpenFlags.OpenUsePacketScanning);
					}
					else
					{
						// No XMP was found, return to using smart handler
						xmpFiles.CloseFile(CloseFlags.None);
						tempAviFilePath = Path.GetTempFileName();
						File.Copy(filePath, tempAviFilePath, true);
						xmpFiles.OpenFile(filePath, FileFormat.Unknown, smartScanningFlags);
					}
				}
			}
	
			//if (!xmpExists && fileMode != XmpFileMode.WriteOnly)
			//{
			//    throw new ApplicationException("Failed to read XMP from file.");
			//}

			commonFields = new CommonFields();
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				if (fileMode == XmpFileMode.ReadOnly)
				{
					xmpFiles.CloseFile(CloseFlags.None);
				}
				else
				{
					// Work-around for safe writing of AVI-files.
					if (tempAviFilePath != null)
					{
						// Write XMP to temporary file
						XmpFiles xmpFilesAvi = new XmpFiles();
						xmpFilesAvi.OpenFile(tempAviFilePath, FileFormat.Avi, OpenFlags.OpenForUpdate | OpenFlags.OpenUseSmartHandler);
						xmpFilesAvi.PutXmp(xmpCore);
						xmpFilesAvi.CloseFile(CloseFlags.None);
						xmpFilesAvi.Dispose();

						// Open temporary file with smart handler and see if packet was found.
						xmpFilesAvi = new XmpFiles();
						xmpFilesAvi.OpenFile(tempAviFilePath, FileFormat.Avi, OpenFlags.OpenForRead | OpenFlags.OpenUseSmartHandler);
						try
						{
							if (!xmpFilesAvi.GetXmp())
							{
								throw new ApplicationException("The AVI file does not support smart handler writing.");
							}
						}
						finally
						{
							xmpFilesAvi.CloseFile(CloseFlags.None);
							xmpFilesAvi.Dispose();
							File.Delete(tempAviFilePath);
						}
					}

					CloseFlags closeFlags = CloseFlags.None;

					OpenFlags openFlags;
					FileFormat fileFormat;
					FormatInfo formatInfo;
					if (xmpFiles.GetFileInfo(out openFlags, out fileFormat, out formatInfo))
					{
						if ((formatInfo & FormatInfo.AllowsSafeUpdate) == FormatInfo.AllowsSafeUpdate)
						{
							closeFlags = CloseFlags.UpdateSafely;
						}
					}

					xmpFiles.CloseFile(closeFlags);
				}
				xmpCore.Dispose();
				xmpFiles.Dispose();

				lock (lockObject)
				{
					initializeCount--;
					if (initializeCount == 0)
					{
						XmpFiles.Terminate();
						XmpCore.Terminate();
					}
				}
			}
		}

		private XmpCore xmpCore;
		/// <summary>
		/// 
		/// </summary>
		public XmpCore XmpCore
		{
			get
			{
				if (disposed)
				{
					throw new ObjectDisposedException("Xmp");
				}
				return xmpCore;
			}
		}

		private CommonFields commonFields;
		/// <summary>
		/// 
		/// </summary>
		internal CommonFields CommonFields
		{
			get { return commonFields; }
		}

		public Image Thumbnail
		{
			get
			{
				ThumbnailInfo thumbnailInfo;
				if (xmpFiles.GetThumbnail(out thumbnailInfo))
				{
					return thumbnailInfo.GetThumbnail();
				}
				else
				{
					return null;
				}
			}
			set
			{
				throw new NotImplementedException();
				//thumbnail = value;
			}
		}

		private static object lockObject = new object();
		private volatile static int initializeCount = 0;

		private volatile bool disposed = false;
		private string filePath;
		private XmpFileMode fileMode;
		private XmpFiles xmpFiles;

		// Work-around for AVI files
		private string tempAviFilePath = null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="fileMode"></param>
		/// <returns></returns>
		public static Xmp FromFile(string filePath, XmpFileMode fileMode)
		{
			Xmp xmp = new Xmp(filePath, fileMode);
			return xmp;
		}

		public string Dump()
		{
			StringBuilder xmpDump = new StringBuilder();
			xmpCore.DumpObject(new TextOutputDelegate(TextOutput), xmpDump);
			return xmpDump.ToString();
		}

		/// <summary>
		/// Saves the XMP to file.
		/// </summary>
		/// <returns></returns>
		public bool Save()
		{
			if (disposed)
			{
				throw new ObjectDisposedException("Xmp");
			}

			if (fileMode == XmpFileMode.ReadOnly)
			{
				throw new ApplicationException("File was opened as read-only.");
			}
			else
			{
				if (xmpFiles.CanPutXmp(xmpCore))
				{
					xmpFiles.PutXmp(xmpCore);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		private void TextOutput(object state, string textOutput)
		{
			StringBuilder sb = (StringBuilder)state;
			sb.Append(textOutput);
		}
	}
}
