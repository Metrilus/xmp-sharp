using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// 
	/// </summary>
	public class ThumbnailInfo
	{
		internal ThumbnailInfo()
		{
		}

		private FileFormat fileFormat = FileFormat.Unknown;
		/// <summary>
		/// The format of the containing file.
		/// </summary>
		public FileFormat FileFormat
		{
			get { return fileFormat; }
			internal set { fileFormat = value; }
		}

		private int fullWidth = 0;
		/// <summary>
		/// Full image width in pixels.
		/// </summary>
		public int FullWidth
		{
			get { return fullWidth; }
			internal set { fullWidth = value; }
		}

		private int fullHeight = 0;
		/// <summary>
		/// Full image height in pixels.
		/// </summary>
		public int FullHeight
		{
			get { return fullHeight; }
			internal set { fullHeight = value; }
		}

		private int thumbnailWidth = 0;
		/// <summary>
		/// Thumbnail image width in pixels.
		/// </summary>
		public int ThumbnailWidth
		{
			get { return thumbnailWidth; }
			internal set { thumbnailWidth = value; }
		}

		private int thumbnailHeight = 0;
		/// <summary>
		/// Thumbnail image height in pixels.
		/// </summary>
		public int ThumbnailHeight
		{
			get { return thumbnailHeight; }
			internal set { thumbnailHeight = value; }
		}

		private short fullOrientation = 0;
		/// <summary>
		/// Orientation of full image, as defined by Exif for tag 274.
		/// </summary>
		public short FullOrientation
		{
			get { return fullOrientation; }
			internal set { fullOrientation = value; }
		}

		private short thumbnailOrientation = 0;
		/// <summary>
		/// Orientation of thumbnail, as defined by Exif for tag 274.
		/// </summary>
		public short ThumbnailOrientation
		{
			get { return thumbnailOrientation; }
			internal set { thumbnailOrientation = value; }
		}

		private IntPtr thumbnailImage = IntPtr.Zero;
		/// <summary>
		/// Raw data from the host file, valid for life of the owning XMPFiles object. Do not modify!
		/// </summary>
		internal IntPtr ThumbnailImage
		{
			get { return thumbnailImage; }
			set { thumbnailImage = value; }
		}

		private int thumbnailSize = 0;
		/// <summary>
		/// The size in bytes of the tnailImage data.
		/// </summary>
		public int ThumbnailSize
		{
			get { return thumbnailSize; }
			internal set { thumbnailSize = value; }
		}

		private ThumbnailFormat thumbnailFormat = ThumbnailFormat.Unknown;
		/// <summary>
		/// The format of the tnailImage data.
		/// </summary>
		public ThumbnailFormat ThumbnailFormat
		{
			get { return thumbnailFormat; }
			internal set { thumbnailFormat = value; }
		}

		/// <summary>
		/// Gets the thumbnails as a System.Drawing.Image
		/// </summary>
		/// <returns>The thumbnail from the file, or null if it could not be read/parsed.</returns>
		public Image GetThumbnail()
		{
			if (thumbnailImage != IntPtr.Zero && 
				thumbnailSize > 0)
			{
				MemoryStream thumbnailStream = null;
				Image thumbnail = null;
				string tempFileName = null;
				try
				{
					byte[] thumbnailBuffer = new byte[thumbnailSize];
					Marshal.Copy(thumbnailImage, thumbnailBuffer, 0, thumbnailBuffer.Length);
					thumbnailStream = new MemoryStream(thumbnailBuffer);
					thumbnail = Image.FromStream(thumbnailStream);
					tempFileName = Path.GetTempFileName();
					thumbnail.Save(tempFileName, ImageFormat.Jpeg);
				}
				catch
				{
					tempFileName = null;
				}
				finally
				{
					if (thumbnail != null)
					{
						thumbnail.Dispose();
					}
					if (thumbnailStream != null)
					{
						thumbnailStream.Dispose();
					}
				}

				if (tempFileName != null)
				{
					return Image.FromFile(tempFileName);
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}
	}
}
