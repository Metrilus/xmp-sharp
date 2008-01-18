using System;
using System.Collections.Generic;
using System.Text;

namespace System.Runtime.InteropServices
{
	internal static class MarshalHelper
	{
		/// <summary>
		/// Reads and returns a null-terminated-string from the pointer.
		/// </summary>
		/// <param name="ptr">Pointer to extract string from.</param>
		/// <param name="index">Index to start reading from.</param>
		/// <param name="maxBytes">Maximum number of bytes to read.</param>
		/// <param name="encoding">Encoding to use.</param>
		/// <returns></returns>
		public static string GetString(IntPtr ptr, int index, int maxBytes, Encoding encoding)
		{
			if (ptr == IntPtr.Zero)
			{
				return string.Empty;
			}

			List<byte> bytes = new List<byte>(maxBytes);
			int endIndex = index + maxBytes;
			for (int n = index; n < endIndex; n++)
			{
				byte b = Marshal.ReadByte(ptr, n);
				if (b == 0)
				{
					break;
				}
				bytes.Add(b);
			}
			return encoding.GetString(bytes.ToArray(), 0, bytes.Count);
		}

		/// <summary>
		/// Reads and returns a null-terminated-string from the pointer.
		/// </summary>
		/// <param name="ptr">Pointer to extract string from.</param>
		/// <param name="index">Index to start reading from.</param>
		/// <param name="maxBytes">Maximum number of bytes to read.</param>
		/// <param name="bytesRead">The actual number of bytes read.</param>
		/// <param name="encoding">Encoding to use.</param>
		/// <returns></returns>
		public static string GetString(IntPtr ptr, int index, int maxBytes, out int bytesRead, Encoding encoding)
		{
			if (ptr == IntPtr.Zero)
			{
                bytesRead = 0;
				return string.Empty;
			}

			List<byte> bytes = new List<byte>(maxBytes);
			int endIndex = index + maxBytes;
			bytesRead = maxBytes;
			for (int n = index; n < endIndex; n++)
			{
				byte b = Marshal.ReadByte(ptr, n);
				if (b == 0)
				{
					bytesRead = n - index;
					break;
				}
				bytes.Add(b);
			}
			return encoding.GetString(bytes.ToArray(), 0, bytes.Count);
		}

		public static IntPtr GetString(string value, Encoding encoding)
		{
			byte[] bytes = encoding.GetBytes(value);
			IntPtr pValue = Marshal.AllocHGlobal(bytes.Length + 1);
			Marshal.Copy(bytes, 0, pValue, bytes.Length);
			Marshal.WriteByte(pValue, bytes.Length, 0);
			return pValue;
		}

		public static IntPtr GetString(string value, Encoding encoding, out int size)
		{
			byte[] bytes = encoding.GetBytes(value);
			size = bytes.Length;
			IntPtr pValue = Marshal.AllocHGlobal(bytes.Length + 1);
			Marshal.Copy(bytes, 0, pValue, bytes.Length);
			Marshal.WriteByte(pValue, bytes.Length, 0);
			return pValue;
		}

		public static void FreeString(IntPtr value)
		{
			if (value != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(value);
				value = IntPtr.Zero;
			}
		}
	}
}