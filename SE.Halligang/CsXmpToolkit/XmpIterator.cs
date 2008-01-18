using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// XMP Toolkit iteration services.
	/// </summary>
	public class XmpIterator : IDisposable
	{
		#region Constructor

		/// <summary>
		/// Construct an iterator for the properties within an XMP object.
		/// </summary>
		/// <param name="xmpCore">The XMP object over which to iterate.</param>
		/// <param name="schemaNS">Optional schema namespace URI to restrict the iteration. Omitted (visit all schema) by passing null or "".</param>
		/// <param name="propName">Optional property name to restrict the iteration. May be an arbitrary path expression. Omitted (visit all properties) by passing 0 or "".</param>
		/// <param name="options">Option flags to control the iteration.</param>
		public XmpIterator(XmpCore xmpCore, string schemaNS, string propName, IteratorMode options)
		{
			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;

			try
			{
				if (schemaNS != null && schemaNS.Length > 0)
				{
					pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				}
				if (propName != null && propName.Length > 0)
				{
					pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				}
				xmpIteratorHandle = XMPIterator_Construct(xmpCore.Handle, pSchemaNS, pPropName, options);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pPropName);
			}
		}

		/// <summary>
		/// Disposes all resources allocated by this object.
		/// </summary>
		public void Dispose()
		{
			if (!disposed)
			{
				if (xmpIteratorHandle != IntPtr.Zero)
				{
					XMPIterator_Destruct(xmpIteratorHandle);
					xmpIteratorHandle = IntPtr.Zero;
				}
				disposed = true;
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPIterator_Construct", CharSet = CharSet.Auto)]
		private static extern IntPtr XMPIterator_Construct(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, IteratorMode options);

		[DllImport("XmpToolkit", EntryPoint = "XMPIterator_Destruct", CharSet = CharSet.Auto)]
		private static extern void XMPIterator_Destruct(IntPtr xmpIteratorHandle);

		#endregion

		#region Fields

		private bool disposed = false;
		private IntPtr xmpIteratorHandle = IntPtr.Zero;

		#endregion

		#region Methods

		/// <summary>
		/// Visit the next node in the iteration.
		/// </summary>
		/// <param name="schemaNS">The schema namespace URI of the current property.</param>
		/// <param name="propPath">The XPath name of the current property.</param>
		/// <param name="propValue">The value of the current property.</param>
		/// <param name="options">The flags describing the current property.</param>
		/// <returns>Returns true if there was another node to visit, false if the iteration is done.</returns>
		public bool Next(out string schemaNS, out string propPath, out string propValue, out PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS;
			int schemaNSLength;
			IntPtr pPropPath;
			int propPathLength;
			IntPtr pPropValue;
			int propValueLength;

			if (XMPIterator_Next(xmpIteratorHandle, out pSchemaNS, out schemaNSLength, out pPropPath, out propPathLength,
				out pPropValue, out propValueLength, out options))
			{
				if (schemaNSLength > 0 && pSchemaNS != IntPtr.Zero)
				{
					schemaNS = MarshalHelper.GetString(pSchemaNS, 0, schemaNSLength, Encoding.UTF8);
					Common_FreeString(pSchemaNS);
				}
				else
				{
					schemaNS = string.Empty;
				}

				if (propPathLength > 0 && pPropPath != IntPtr.Zero)
				{
					propPath = MarshalHelper.GetString(pPropPath, 0, propPathLength, Encoding.UTF8);
					Common_FreeString(pPropPath);
				}
				else
				{
					propPath = string.Empty;
				}

				if (propValueLength > 0 && pPropValue != IntPtr.Zero)
				{
					propValue = MarshalHelper.GetString(pPropValue, 0, propValueLength, Encoding.UTF8);
					Common_FreeString(pPropValue);
				}
				else
				{
					propValue = string.Empty;
				}

				return true;
			}
			else
			{
				schemaNS = null;
				propPath = null;
				propValue = null;
				return false;
			}
		}

		/// <summary>
		/// Skip some portion of the remaining iterations.
		/// </summary>
		/// <param name="options">Option flags to control the iteration.</param>
		public void Skip(IteratorSkipMode options)
		{
			AssertValidState();
			XMPIterator_Skip(xmpIteratorHandle, options);
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPIterator_Next", CharSet = CharSet.Auto)]
		private static extern bool XMPIterator_Next(IntPtr xmpIteratorHandle, out IntPtr schemaNS, out int schemaNSLength,
			out IntPtr propPath, out int propPathLength, out IntPtr propValue, out int propValueLength, out PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPIterator_Skip", CharSet = CharSet.Auto)]
		private static extern void XMPIterator_Skip(IntPtr xmpIteratorHandle, IteratorSkipMode options);

		private void AssertValidState()
		{
            if (disposed)
            {
                throw new ObjectDisposedException("Object has been disposed.");
            }
			if (xmpIteratorHandle == IntPtr.Zero)
			{
				throw new NullReferenceException("XMPIterator is null.");
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "Common_FreeString", CharSet = CharSet.Auto)]
		private static extern void Common_FreeString(IntPtr stringHandle);

		[DllImport("XmpToolkit", EntryPoint = "Common_GetLastError", CharSet = CharSet.Auto)]
		private static extern int Common_GetLastError();

		#endregion
	}
}
