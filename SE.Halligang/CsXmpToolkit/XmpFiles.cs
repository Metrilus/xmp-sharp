using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// API for access to the "main" metadata in a file.
	/// </summary>
	public class XmpFiles : IDisposable
	{
		#region Constructor

		/// <summary>
		/// The default constructor initializes an object that is associated with no file.
		/// </summary>
		public XmpFiles()
		{
			if (!initialized)
			{
				throw new InvalidOperationException("XmpFiles.Initialize must be called before constructing XmpFiles objects.");
			}
			xmpFilesHandle = XMPFiles_Construct1();
		}

		/// <summary>
		/// This alternate constructor call OpenFile.
		/// </summary>
		/// <param name="filePath">The path for the file.</param>
		/// <param name="fileFormat">The format of the file.</param>
		/// <param name="openFlags">A set of option bits describing the desired access.</param>
		public XmpFiles(string filePath, FileFormat fileFormat, OpenFlags openFlags)
		{
			if (!initialized)
			{
				throw new InvalidOperationException("XmpFiles.Initialize must be called before constructing XmpFiles objects.");
			}
			IntPtr pFilePath = IntPtr.Zero;
			try
			{
				pFilePath = MarshalHelper.GetString(filePath, Encoding.UTF8);
				xmpFilesHandle = XMPFiles_Construct2(pFilePath, fileFormat, openFlags);
			}
			catch
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pFilePath);
			}
		}
		
		/// <summary>
		/// Disposes all resources allocated by this object.
		/// </summary>
		public void Dispose()
		{
			if (!disposed)
			{
				if (xmpFilesHandle != IntPtr.Zero)
				{
					XMPFiles_Destruct(xmpFilesHandle);
					xmpFilesHandle = IntPtr.Zero;
				}
				disposed = true;
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_Construct1", CharSet = CharSet.Auto)]
		private static extern IntPtr XMPFiles_Construct1();

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_Construct2", CharSet = CharSet.Auto)]
		private static extern IntPtr XMPFiles_Construct2(IntPtr filePath, FileFormat fileFormat, OpenFlags openFlags);

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_Destruct", CharSet = CharSet.Auto)]
		private static extern void XMPFiles_Destruct(IntPtr xmpFilesHandle);

		#endregion

		#region Fields

		private static bool initialized = false;
		private bool disposed = false;
		private IntPtr xmpFilesHandle = IntPtr.Zero;

		#endregion

		#region Methods

		#region Public Member Functions

		#region OpenFile, CloseFile, and related file-oriented operations

		/// <summary>
		/// Open a file for metadata access.
		/// </summary>
		/// <param name="filePath">The path for the file.</param>
		/// <param name="fileFormat">The format of the file.</param>
		/// <param name="openFlags">A set of option bits describing the desired access.</param>
		/// <returns>
		/// Returns true if the file is succesfully opened and attached to a file handler.
		/// </returns>
		public bool OpenFile(string filePath, FileFormat fileFormat, OpenFlags openFlags)
		{
			AssertValidState();
			IntPtr pFilePath = IntPtr.Zero;
			bool result = false;
			try
			{
				pFilePath = MarshalHelper.GetString(filePath, Encoding.UTF8);
				result = XMPFiles_OpenFile(xmpFilesHandle, pFilePath, fileFormat, openFlags);
			}
			catch
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pFilePath);
			}
			return result;
		}

		/// <summary>
		/// Close an opened file.
		/// </summary>
		/// <param name="closeFlags">A set of bit flags for optional closing actions.</param>
		public void CloseFile(CloseFlags closeFlags)
		{
			AssertValidState();
			XMPFiles_CloseFile(xmpFilesHandle, closeFlags);
		}

		/// <summary>
		/// Get basic information about an opened file. 
		/// </summary>
		/// <param name="openFlags">Returns the flags passed to OpenFile.</param>
		/// <param name="fileFormat">Returns the format of the file.</param>
		/// <param name="handlerFlags">Returns the handler's capability flags.</param>
		/// <returns>
		/// Returns true if a file is opened, false otherwise.
		/// </returns>
		public bool GetFileInfo(out OpenFlags openFlags, out FileFormat fileFormat, out FormatInfo handlerFlags)
        {
            AssertValidState();
            return XMPFiles_GetFileInfo(xmpFilesHandle, IntPtr.Zero, out openFlags, out fileFormat, out handlerFlags);
		}

		/// <summary>
		/// Set the callback function used to check for a user signaled abort.
		/// </summary>
		/// <param name="abortProc">The callback function used to check for a user signaled abort.</param>
		/// <param name="abortArg">An argument passed to the callback function.</param>
		public void SetAbortProc(AbortDelegate abortProc, IntPtr abortArg)
		{
			AssertValidState();
			XMPFiles_SetAbortProc(xmpFilesHandle, abortProc, abortArg);
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_OpenFile", CharSet = CharSet.Auto)]
		private static extern bool XMPFiles_OpenFile(IntPtr xmpFilesHandle, IntPtr filePath, FileFormat fileFormat, OpenFlags openFlags);

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_CloseFile", CharSet = CharSet.Auto)]
		private static extern void XMPFiles_CloseFile(IntPtr xmpFilesHandle, CloseFlags closeFlags);

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_GetFileInfo", CharSet = CharSet.Auto)]
		private static extern bool XMPFiles_GetFileInfo(IntPtr xmpFilesHandle, IntPtr filePath, out OpenFlags openFlags, out FileFormat fileFormat, out FormatInfo handlerFlags);

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_SetAbortProc", CharSet = CharSet.Auto)]
		private static extern bool XMPFiles_SetAbortProc(IntPtr xmpFilesHandle, AbortDelegate abortProc, IntPtr abortArg);

		#endregion

		#region Metadata Access Functions

		/// <summary>
		/// Obtain the XMP.
		/// </summary>
		/// <returns>Returns true if the file has XMP, false otherwise. </returns>
		public bool GetXmp()
		{
			AssertValidState();
			return XMPFiles_GetXMP(xmpFilesHandle, IntPtr.Zero, false, IntPtr.Zero, 0, IntPtr.Zero);
		}

		/// <summary>
		/// Obtain the XMP.
		/// </summary>
		/// <param name="xmpCore">Returns the parsed XMP.</param>
		/// <returns>Returns true if the file has XMP, false otherwise. </returns>
		public bool GetXmp(XmpCore xmpCore)
		{
			AssertValidState();

			IntPtr xmpCoreHandle;
			if (xmpCore == null)
			{
				xmpCoreHandle = IntPtr.Zero;
			}
			else
			{
				xmpCoreHandle = xmpCore.Handle;
			}

			try
			{
				bool result = XMPFiles_GetXMP(xmpFilesHandle, xmpCoreHandle, false, IntPtr.Zero, 0, IntPtr.Zero);
				return result;
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
		}

		/// <summary>
		/// Obtain the XMP.
		/// </summary>
		/// <param name="xmpCore">Returns the parsed XMP.</param>
		/// <param name="packetInfo">Returns the location and form of the raw XMP in the file.</param>
		/// <returns>Returns true if the file has XMP, false otherwise. </returns>
		public bool GetXmp(XmpCore xmpCore, PacketInfo packetInfo)
		{
			AssertValidState();

			IntPtr xmpCoreHandle;
			if (xmpCore == null)
			{
				xmpCoreHandle = IntPtr.Zero;
			}
			else
			{
				xmpCoreHandle = xmpCore.Handle;
			}
			PInvoke.PacketInfo pPacketInfo = new PInvoke.PacketInfo();

			if (XMPFiles_GetXMP(xmpFilesHandle, xmpCoreHandle, false, IntPtr.Zero, 0, ref pPacketInfo))
			{
				if (packetInfo != null)
				{
					packetInfo.Offset = pPacketInfo.Offset;
					packetInfo.Length = pPacketInfo.Length;
					packetInfo.PadSize = pPacketInfo.PadSize;
					packetInfo.CharForm = pPacketInfo.CharForm;
					packetInfo.Writeable = (pPacketInfo.Writeable != 0);
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Obtain the XMP.
		/// </summary>
		/// <param name="xmpCore">Returns the parsed XMP.</param>
		/// <param name="packetInfo">Returns the location and form of the raw XMP in the file.</param>
		/// <param name="xmpPacket">Returns the raw XMP packet as stored in the file.</param>
		/// <returns>Returns true if the file has XMP, false otherwise. </returns>
		public bool GetXmp(XmpCore xmpCore, PacketInfo packetInfo, out string xmpPacket)
		{
			AssertValidState();

			IntPtr xmpCoreHandle;
			if (xmpCore == null)
			{
				xmpCoreHandle = IntPtr.Zero;
			}
			else
			{
				xmpCoreHandle = xmpCore.Handle;
			}
			IntPtr pXmpPacket;
			int xmpPacketLength;
			PInvoke.PacketInfo pPacketInfo = new PInvoke.PacketInfo();
			
			if (XMPFiles_GetXMP(xmpFilesHandle, xmpCoreHandle, true, out pXmpPacket, out xmpPacketLength, ref pPacketInfo))
			{
				if (xmpPacketLength > 0 && pXmpPacket != IntPtr.Zero)
				{
					xmpPacket = MarshalHelper.GetString(pXmpPacket, 0, xmpPacketLength, Encoding.UTF8);
					Common_FreeString(pXmpPacket);
				}
				else
				{
					xmpPacket = string.Empty;
				}

				if (packetInfo != null)
				{
					packetInfo.Offset = pPacketInfo.Offset;
					packetInfo.Length = pPacketInfo.Length;
					packetInfo.PadSize = pPacketInfo.PadSize;
					packetInfo.CharForm = pPacketInfo.CharForm;
					packetInfo.Writeable = (pPacketInfo.Writeable != 0);
				}
				return true;
			}
			else
			{
				xmpPacket = null;
				return false;
			}
		}

		/// <summary>
		/// Obtain the native thumbnail.
		/// </summary>
		/// <param name="thumbnailInfo">Returns information about a recognized native thumbnail, and some related information about the primary image if appropriate.</param>
		/// <returns>Returns true if a recognized native thumbnail is present and the thumbnail was cached by OpenFile.</returns>
		public bool GetThumbnail(out ThumbnailInfo thumbnailInfo)
		{
			AssertValidState();
			PInvoke.ThumbnailInfo pThumbnailInfo = new PInvoke.ThumbnailInfo();
			if (XMPFiles_GetThumbnail(xmpFilesHandle, ref pThumbnailInfo))
			{
				thumbnailInfo = new ThumbnailInfo();
				thumbnailInfo.FileFormat = pThumbnailInfo.FileFormat;
				thumbnailInfo.FullWidth = pThumbnailInfo.FullWidth;
				thumbnailInfo.FullHeight = pThumbnailInfo.FullHeight;
				thumbnailInfo.ThumbnailWidth = pThumbnailInfo.ThumbnailWidth;
				thumbnailInfo.ThumbnailHeight = pThumbnailInfo.ThumbnailHeight;
				thumbnailInfo.FullOrientation = pThumbnailInfo.FullOrientation;
				thumbnailInfo.ThumbnailOrientation = pThumbnailInfo.ThumbnailOrientation;
				thumbnailInfo.ThumbnailImage = pThumbnailInfo.ThumbnailImage;
				thumbnailInfo.ThumbnailSize = pThumbnailInfo.ThumbnailSize;
				thumbnailInfo.ThumbnailFormat = pThumbnailInfo.ThumbnailFormat;
				return true;
			}
			else
			{
				thumbnailInfo = null;
				return false;
			}
		}

		/// <summary>
		/// Update the XMP.
		/// </summary>
		/// <param name="xmpCore">Parsed XMP.</param>
		public void PutXmp(XmpCore xmpCore)
		{
			AssertValidState();
			XMPFiles_PutXMP(xmpFilesHandle, xmpCore.Handle);
		}

		/// <summary>
		/// Determine if the XMP can be updated. 
		/// </summary>
		/// <param name="xmpCore">Parsed XMP.</param>
		/// <returns>True if the XMP can be updated, false otherwise.</returns>
		public bool CanPutXmp(XmpCore xmpCore)
		{
			AssertValidState();
			return XMPFiles_CanPutXMP(xmpFilesHandle, xmpCore.Handle);
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_GetXMP", CharSet = CharSet.Auto)]
		private static extern bool XMPFiles_GetXMP(IntPtr xmpFilesHandle, IntPtr xmpCoreHandle, bool getXmpPacket, IntPtr xmpPacket, int xmpPacketLength, IntPtr packetInfo);

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_GetXMP", CharSet = CharSet.Auto)]
		private static extern bool XMPFiles_GetXMP(IntPtr xmpFilesHandle, IntPtr xmpCoreHandle, bool getXmpPacket, IntPtr xmpPacket, int xmpPacketLength, ref PInvoke.PacketInfo packetInfo);

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_GetXMP", CharSet = CharSet.Auto)]
		private static extern bool XMPFiles_GetXMP(IntPtr xmpFilesHandle, IntPtr xmpCoreHandle, bool getXmpPacket, out IntPtr xmpPacket, out int xmpPacketLength, ref PInvoke.PacketInfo packetInfo);

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_GetThumbnail", CharSet = CharSet.Auto)]
		private static extern bool XMPFiles_GetThumbnail(IntPtr xmpFilesHandle, ref PInvoke.ThumbnailInfo thumbnailInfo);

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_PutXMP", CharSet = CharSet.Auto)]
		private static extern bool XMPFiles_PutXMP(IntPtr xmpFilesHandle, IntPtr xmpCoreHandle);

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_CanPutXMP", CharSet = CharSet.Auto)]
		private static extern bool XMPFiles_CanPutXMP(IntPtr xmpFilesHandle, IntPtr xmpCoreHandle);

		#endregion

		#endregion

		#region Static Public Member Functions

		#region Initialization and termination

		/// <summary>
		/// Gets the version information for the XMP Toolkit.
		/// </summary>
		/// <returns>The version information for the XMP Toolkit.</returns>
		public static XmpVersionInfo GetVersionInfo()
		{
			PInvoke.XmpVersionInfo versionInfo = new PInvoke.XmpVersionInfo();
			XMPFiles_GetVersionInfo(ref versionInfo);
			return new XmpVersionInfo(versionInfo);
		}

		/// <summary>
		/// Initialize must be called before using XmpFiles.
		/// </summary>
		/// <returns>True if initialized, false otherwise.</returns>
		public static bool Initialize()
		{
			if (initialized)
			{
				return true;
			}
			else
			{
				initialized = XMPFiles_Initialize1();
				return initialized;
			}
		}

		/// <summary>
		/// Initialize must be called before using XmpFiles.
		/// </summary>
		/// <param name="options"></param>
		/// <returns>True if initialized, false otherwise.</returns>
		public static bool Initialize(int options)
		{
			throw new NotImplementedException("Initialize with options is not yet implemented.");
		}

		/// <summary>
		/// Terminate may be called when done using XmpFiles. It deallocates global data structures created by Initialize. 
		/// </summary>
		public static void Terminate()
		{
			XMPFiles_Terminate();
			initialized = false;
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_GetVersionInfo", CharSet = CharSet.Auto)]
		private static extern void XMPFiles_GetVersionInfo(ref PInvoke.XmpVersionInfo xmpVersionInfo);

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_Initialize1", CharSet = CharSet.Auto)]
		private static extern bool XMPFiles_Initialize1();

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_Initialize2", CharSet = CharSet.Auto)]
		private static extern bool XMPFiles_Initialize2(int optionBits);

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_Terminate", CharSet = CharSet.Auto)]
		private static extern void XMPFiles_Terminate();

		#endregion

		#region Static Functions

		/// <summary>
		/// Determine the supported features for a given file format.
		/// </summary>
		/// <param name="fileFormat">File format.</param>
		/// <param name="handlerFlags">Returns the handler flags for the file format.</param>
		/// <returns>True if format info was retrieved, false otherwise.</returns>
		public static bool GetFormatInfo(FileFormat fileFormat, out FormatInfo handlerFlags)
		{
			return XMPFiles_GetFormatInfo(fileFormat, out handlerFlags);
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPFiles_GetFormatInfo", CharSet = CharSet.Auto)]
		private static extern bool XMPFiles_GetFormatInfo(FileFormat fileFormat, out FormatInfo formatInfo);

		#endregion

		#endregion

		private void AssertValidState()
		{
            if (disposed)
            {
                throw new ObjectDisposedException("Object has been disposed.");
            }
            if (!initialized)
			{
				throw new InvalidOperationException("Initialize must be called before using an XmpFiles object.");
			}
			if (xmpFilesHandle == IntPtr.Zero)
			{
				throw new NullReferenceException("XmpFiles is null.");
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "Common_FreeString", CharSet = CharSet.Auto)]
		private static extern void Common_FreeString(IntPtr stringHandle);

		[DllImport("XmpToolkit", EntryPoint = "Common_GetLastError", CharSet = CharSet.Auto)]
		private static extern int Common_GetLastError();

		#endregion
	}
}
