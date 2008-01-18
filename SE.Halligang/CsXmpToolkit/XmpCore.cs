using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// 
	/// </summary>
	public class XmpCore : IDisposable
	{
		#region Constructor

		public XmpCore()
		{
			if (!initialized)
			{
				throw new InvalidOperationException("Initialize must be called before constructing an XmpCore object.");
			}
			xmpCoreHandle = XMPMeta_Construct1();
		}

		public void Dispose()
		{
			if (!disposed)
			{
				if (xmpCoreHandle != IntPtr.Zero)
				{
					XMPMeta_Destruct(xmpCoreHandle);
					xmpCoreHandle = IntPtr.Zero;
				}
				disposed = true;
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_Construct1", CharSet = CharSet.Auto)]
		private static extern IntPtr XMPMeta_Construct1();

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_Destruct", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_Destruct(IntPtr metaHandle);

		#endregion

		#region Properties

		private IntPtr xmpCoreHandle = IntPtr.Zero;
		/// <summary>
		/// 
		/// </summary>
		internal IntPtr Handle
		{
			get { return xmpCoreHandle; }
			set { xmpCoreHandle = value; }
		}	

		#endregion

		#region Fields

		private static bool initialized = false;
		private bool disposed = false;

		#endregion

		#region Methods

		#region Public Member Functions 

		#region Functions for getting property values 

		public bool GetProperty(string schemaNS, string propName, out string propValue, out PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;

			IntPtr pPropValue;
			int propValueLength;
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				if (XMPMeta_GetProperty(xmpCoreHandle, pSchemaNS, pPropName, out pPropValue, out propValueLength, out options))
				{
					if (propValueLength > 0 && pPropValue != IntPtr.Zero)
					{
						propValue = MarshalHelper.GetString(pPropValue, 0, propValueLength, Encoding.UTF8);
						Common_FreeString(pPropValue);
					}
					else
					{
						propValue = null;
					}
					result = true;
				}
				else
				{
					propValue = null;
					result = false;
				}
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
			return result;
		}

		public bool GetArrayItem(string schemaNS, string arrayName, int itemIndex, out string itemValue, out PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pArrayName = IntPtr.Zero;

			IntPtr pItemValue;
			int itemValueLength;
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pArrayName = MarshalHelper.GetString(arrayName, Encoding.UTF8);
				if (XMPMeta_GetArrayItem(xmpCoreHandle, pSchemaNS, pArrayName, itemIndex, out pItemValue, out itemValueLength, out options))
				{
					if (itemValueLength > 0 && pItemValue != IntPtr.Zero)
					{
						itemValue = MarshalHelper.GetString(pItemValue, 0, itemValueLength, Encoding.UTF8);
						Common_FreeString(pItemValue);
					}
					else
					{
						itemValue = null;
					}
					result = true;
				}
				else
				{
					itemValue = null;
					result = false;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pArrayName);
			}
			return result;
		}

		public bool GetStructField(string schemaNS, string structName, string fieldNS, string fieldName, out string fieldValue, out PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pStructName = IntPtr.Zero;
			IntPtr pFieldNS = IntPtr.Zero;
			IntPtr pFieldName = IntPtr.Zero;

			IntPtr pFieldValue;
			int fieldValueLength;
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pStructName = MarshalHelper.GetString(structName, Encoding.UTF8);
				pFieldNS = MarshalHelper.GetString(fieldNS, Encoding.UTF8);
				pFieldName = MarshalHelper.GetString(fieldName, Encoding.UTF8);
				if (XMPMeta_GetStructField(xmpCoreHandle, pSchemaNS, pStructName, pFieldNS, pFieldName, out pFieldValue, out fieldValueLength, out options))
				{
					if (fieldValueLength > 0 && pFieldValue != IntPtr.Zero)
					{
						fieldValue = MarshalHelper.GetString(pFieldValue, 0, fieldValueLength, Encoding.UTF8);
						Common_FreeString(pFieldValue);
					}
					else
					{
						fieldValue = null;
					}
					result = true;
				}
				else
				{
					fieldValue = null;
					result = false;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pStructName);
				MarshalHelper.FreeString(pFieldNS);
				MarshalHelper.FreeString(pFieldName);
			}
			return result;
		}

		public bool GetQualifier(string schemaNS, string propName, string qualNS, string qualName, out string qualValue, out PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;
			IntPtr pQualNS = IntPtr.Zero;
			IntPtr pQualName = IntPtr.Zero;

			IntPtr pQualValue;
			int qualValueLength;
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				pQualNS = MarshalHelper.GetString(qualNS, Encoding.UTF8);
				pQualName = MarshalHelper.GetString(qualName, Encoding.UTF8);
				if (XMPMeta_GetQualifier(xmpCoreHandle, pSchemaNS, pPropName, pQualNS, pQualName, out pQualValue, out qualValueLength, out options))
				{
					if (qualValueLength > 0 && pQualValue != IntPtr.Zero)
					{
						qualValue = MarshalHelper.GetString(pQualValue, 0, qualValueLength, Encoding.UTF8);
						Common_FreeString(pQualValue);
					}
					else
					{
						qualValue = null;
					}
					result = true;
				}
				else
				{
					qualValue = null;
					result = false;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pPropName);
				MarshalHelper.FreeString(pQualNS);
				MarshalHelper.FreeString(pQualName);
			}
			return result;
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetProperty", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_GetProperty(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, out IntPtr propValue, out int propValueLength, out PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetArrayItem", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_GetArrayItem(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr arrayName, int itemIndex, out IntPtr itemValue, out int itemValueLength, out PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetStructField", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_GetStructField(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr structName, IntPtr fieldNS, IntPtr fieldName, out IntPtr fieldValue, out int fieldValueLength, out PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetQualifier", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_GetQualifier(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, IntPtr qualNS, IntPtr qualName, out IntPtr qualValue, out int qualValueLength, out PropertyFlags options);

		#endregion

		#region Functions for setting property values

		/// <summary>
		/// 
		/// </summary>
		/// <param name="schemaNS"></param>
		/// <param name="propName"></param>
		/// <param name="propValue"></param>
		/// <param name="options"></param>
		public void SetProperty(string schemaNS, string propName, string propValue, PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;
			IntPtr pPropValue = IntPtr.Zero;

			LogFile log = LogFile.GetInstance("CsXmpToolkit");
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				if (propValue != null)
				{
					pPropValue = MarshalHelper.GetString(propValue, Encoding.UTF8);
				}
				log.AppendString(TraceLevel.Verbose, MethodInfo.GetCurrentMethod(), propName + " = " + (propValue == null ? "null" : propValue));
				XMPMeta_SetProperty(xmpCoreHandle, pSchemaNS, pPropName, pPropValue, options);
			}
			catch (Exception ex)
			{
				log.AppendException(TraceLevel.Error, MethodInfo.GetCurrentMethod(), ex);
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Marshal.GetLastWin32Error(), ex);
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pPropName);
				if (pPropValue != IntPtr.Zero)
				{
					MarshalHelper.FreeString(pPropValue);
				}
			}
		}

		public void SetArrayItem(string schemaNS, string arrayName, int itemIndex, string itemValue, PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pArrayName = IntPtr.Zero;
			IntPtr pItemValue = IntPtr.Zero;

			LogFile log = LogFile.GetInstance("CsXmpToolkit");
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pArrayName = MarshalHelper.GetString(arrayName, Encoding.UTF8);
				if (itemValue != null && itemValue.Length > 0)
				{
					pItemValue = MarshalHelper.GetString(itemValue, Encoding.UTF8);
				}
				log.AppendString(TraceLevel.Verbose, MethodInfo.GetCurrentMethod(), arrayName + "[" + itemIndex.ToString() + "] = " + (itemValue == null ? "null" : itemValue));
				XMPMeta_SetArrayItem(xmpCoreHandle, pSchemaNS, pArrayName, itemIndex, pItemValue, options);
			}
			catch (Exception ex)
			{
				log.AppendException(TraceLevel.Error, MethodInfo.GetCurrentMethod(), ex);
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Marshal.GetLastWin32Error(), ex);
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pArrayName);
				if (pItemValue != IntPtr.Zero)
				{
					MarshalHelper.FreeString(pItemValue);
				}
			}
		}

		public void AppendArrayItem(string schemaNS, string arrayName, PropertyFlags arrayOptions, string itemValue, PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pArrayName = IntPtr.Zero;
			IntPtr pItemValue = IntPtr.Zero;

			LogFile log = LogFile.GetInstance("CsXmpToolkit");
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pArrayName = MarshalHelper.GetString(arrayName, Encoding.UTF8);
				if (itemValue != null)
				{
					pItemValue = MarshalHelper.GetString(itemValue, Encoding.UTF8);
				}
				log.AppendString(TraceLevel.Verbose, MethodInfo.GetCurrentMethod(), arrayName + ".Add(" + (itemValue == null ? "null" : itemValue) + ")");
				XMPMeta_AppendArrayItem(xmpCoreHandle, pSchemaNS, pArrayName, arrayOptions, pItemValue, options);
			}
			catch (Exception ex)
			{
				log.AppendException(TraceLevel.Error, MethodInfo.GetCurrentMethod(), ex);
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Marshal.GetLastWin32Error(), ex);
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pArrayName);
				if (pItemValue != IntPtr.Zero)
				{
					MarshalHelper.FreeString(pItemValue);
				}
			}			
		}

		public void SetStructField(string schemaNS, string structName, string fieldNS, string fieldName, string fieldValue, PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pStructName = IntPtr.Zero;
			IntPtr pFieldNS = IntPtr.Zero;
			IntPtr pFieldName = IntPtr.Zero;
			IntPtr pFieldValue = IntPtr.Zero;

			LogFile log = LogFile.GetInstance("CsXmpToolkit");
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pStructName = MarshalHelper.GetString(structName, Encoding.UTF8);
				pFieldNS = MarshalHelper.GetString(fieldNS, Encoding.UTF8);
				pFieldName = MarshalHelper.GetString(fieldName, Encoding.UTF8);
				pFieldValue = MarshalHelper.GetString(fieldValue, Encoding.UTF8);
				log.AppendString(TraceLevel.Verbose, MethodInfo.GetCurrentMethod(), structName + "." + fieldName + " = " + fieldValue);
				XMPMeta_SetStructField(xmpCoreHandle, pSchemaNS, pStructName, pFieldNS, pFieldName, pFieldValue, options);
			}
			catch (Exception ex)
			{
				log.AppendException(TraceLevel.Error, MethodInfo.GetCurrentMethod(), ex);
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Marshal.GetLastWin32Error(), ex);
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pStructName);
				MarshalHelper.FreeString(pFieldNS);
				MarshalHelper.FreeString(pFieldName);
				MarshalHelper.FreeString(pFieldValue);
			}			
		}

		public void SetQualifier(string schemaNS, string propName, string qualNS, string qualName, string qualValue, PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;
			IntPtr pQualNS = IntPtr.Zero;
			IntPtr pQualName = IntPtr.Zero;
			IntPtr pQualValue = IntPtr.Zero;

			LogFile log = LogFile.GetInstance("CsXmpToolkit");
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				pQualNS = MarshalHelper.GetString(qualNS, Encoding.UTF8);
				pQualName = MarshalHelper.GetString(qualName, Encoding.UTF8);
				if (qualValue != null)
				{
					pQualValue = MarshalHelper.GetString(qualValue, Encoding.UTF8);
				}
				log.AppendString(TraceLevel.Verbose, MethodInfo.GetCurrentMethod(), qualName + "(" + propName + ") = " + (qualValue == null ? "null" : qualValue));
				XMPMeta_SetQualifier(xmpCoreHandle, pSchemaNS, pPropName, pQualNS, pQualName, pQualValue, options);
			}
			catch (Exception ex)
			{
				log.AppendException(TraceLevel.Error, MethodInfo.GetCurrentMethod(), ex);
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Marshal.GetLastWin32Error(), ex);
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pPropName);
				MarshalHelper.FreeString(pQualNS);
				MarshalHelper.FreeString(pQualName);
				if (pQualValue != IntPtr.Zero)
				{
					MarshalHelper.FreeString(pQualValue);
				}
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_SetProperty", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern void XMPMeta_SetProperty(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, IntPtr propValue, PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_SetArrayItem", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern void XMPMeta_SetArrayItem(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr arrayName, int itemIndex, IntPtr itemValue, PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_AppendArrayItem", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern void XMPMeta_AppendArrayItem(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr arrayName, PropertyFlags arrayOptions, IntPtr itemValue, PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_SetStructField", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern void XMPMeta_SetStructField(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr structName, IntPtr fieldNS, IntPtr fieldName, IntPtr fieldValue, PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_SetQualifier", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern void XMPMeta_SetQualifier(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, IntPtr qualNS, IntPtr qualName, IntPtr qualValue, PropertyFlags options);

		#endregion

		#region Functions for deleting and detecting properties

		public void DeleteProperty(string schemaNS, string propName)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;

			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				XMPMeta_DeleteProperty(xmpCoreHandle, pSchemaNS, pPropName);
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

		public void DeleteArrayItem(string schemaNS, string arrayName, int itemIndex)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pArrayName = IntPtr.Zero;

			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pArrayName = MarshalHelper.GetString(arrayName, Encoding.UTF8);
				XMPMeta_DeleteArrayItem(xmpCoreHandle, pSchemaNS, pArrayName, itemIndex);
			}
			catch (XmpException ex)
			{
				throw ex;
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pArrayName);
			}			
		}

		public void DeleteStructField(string schemaNS, string structName, string fieldNS, string fieldName)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pStructName = IntPtr.Zero;
			IntPtr pFieldNS = IntPtr.Zero;
			IntPtr pFieldName = IntPtr.Zero;

			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pStructName = MarshalHelper.GetString(structName, Encoding.UTF8);
				pFieldNS = MarshalHelper.GetString(fieldNS, Encoding.UTF8);
				pFieldName = MarshalHelper.GetString(fieldName, Encoding.UTF8);
				XMPMeta_DeleteStructField(xmpCoreHandle, pSchemaNS, pStructName, pFieldNS, pFieldName);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pStructName);
				MarshalHelper.FreeString(pFieldNS);
				MarshalHelper.FreeString(pFieldName);
			}
		}

		public void DeleteQualifier(string schemaNS, string propName, string qualNS, string qualName)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;
			IntPtr pQualNS = IntPtr.Zero;
			IntPtr pQualName = IntPtr.Zero;

			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				pQualNS = MarshalHelper.GetString(qualNS, Encoding.UTF8);
				pQualName = MarshalHelper.GetString(qualName, Encoding.UTF8);
				XMPMeta_DeleteQualifier(xmpCoreHandle, pSchemaNS, pPropName, pQualNS, pQualName);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pPropName);
				MarshalHelper.FreeString(pQualNS);
				MarshalHelper.FreeString(pQualName);
			}			
		}

		public bool DoesPropertyExist(string schemaNS, string propName)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				result = XMPMeta_DoesPropertyExist(xmpCoreHandle, pSchemaNS, pPropName);
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
			return result;
		}

		public bool DoesArrayItemExist(string schemaNS, string arrayName, int itemIndex)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pArrayName = IntPtr.Zero;
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pArrayName = MarshalHelper.GetString(arrayName, Encoding.UTF8);
				result = XMPMeta_DoesArrayItemExist(xmpCoreHandle, pSchemaNS, pArrayName, itemIndex);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pArrayName);
			}
			return result;
		}

		public bool DoesStructFieldExist(string schemaNS, string structName, string fieldNS, string fieldName)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pStructName = IntPtr.Zero;
			IntPtr pFieldNS = IntPtr.Zero;
			IntPtr pFieldName = IntPtr.Zero;
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pStructName = MarshalHelper.GetString(structName, Encoding.UTF8);
				pFieldNS = MarshalHelper.GetString(fieldNS, Encoding.UTF8);
				pFieldName = MarshalHelper.GetString(fieldName, Encoding.UTF8);
				result = XMPMeta_DoesStructFieldExist(xmpCoreHandle, pSchemaNS, pStructName, pFieldNS, pFieldName);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pStructName);
				MarshalHelper.FreeString(pFieldNS);
				MarshalHelper.FreeString(pFieldName);
			}
			return result;
		}

		public bool DoesQualifierExist(string schemaNS, string propName, string qualNS, string qualName)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;
			IntPtr pQualNS = IntPtr.Zero;
			IntPtr pQualName = IntPtr.Zero;
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				pQualNS = MarshalHelper.GetString(qualNS, Encoding.UTF8);
				pQualName = MarshalHelper.GetString(qualName, Encoding.UTF8);
				result = XMPMeta_DoesQualifierExist(xmpCoreHandle, pSchemaNS, pPropName, pQualNS, pQualName);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pPropName);
				MarshalHelper.FreeString(pQualNS);
				MarshalHelper.FreeString(pQualName);
			}
			return result;
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_DeleteProperty", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_DeleteProperty(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_DeleteArrayItem", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_DeleteArrayItem(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr arrayName, int itemIndex);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_DeleteStructField", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_DeleteStructField(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr structName, IntPtr fieldNS, IntPtr fieldName);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_DeleteQualifier", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_DeleteQualifier(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, IntPtr qualNS, IntPtr qualName);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_DoesPropertyExist", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_DoesPropertyExist(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_DoesArrayItemExist", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_DoesArrayItemExist(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr arrayName, int itemIndex);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_DoesStructFieldExist", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_DoesStructFieldExist(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr structName, IntPtr fieldNS, IntPtr fieldName);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_DoesQualifierExist", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_DoesQualifierExist(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, IntPtr qualNS, IntPtr qualName);

		#endregion

		#region Functions for accessing localized text (alt-text) properties

		public bool GetLocalizedText(string schemaNS, string altTextName, string genericLang, string specificLang, out string actualLang, out string itemValue, out int options)
		{
			AssertValidState();

			IntPtr pActualLang;
			int actualLangLength;
			IntPtr pItemValue;
			int itemValueLength;

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pAltTextName = IntPtr.Zero;
			IntPtr pGenericLang = IntPtr.Zero;
			IntPtr pSpecificLang = IntPtr.Zero;
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pAltTextName = MarshalHelper.GetString(altTextName, Encoding.UTF8);
				pGenericLang = MarshalHelper.GetString(genericLang, Encoding.UTF8);
				pSpecificLang = MarshalHelper.GetString(specificLang, Encoding.UTF8);
				if (XMPMeta_GetLocalizedText(xmpCoreHandle, pSchemaNS, pAltTextName, pGenericLang, pSpecificLang, out pActualLang, out actualLangLength, out pItemValue, out itemValueLength, out options))
				{
					if (actualLangLength > 0 && pActualLang != IntPtr.Zero)
					{
						actualLang = MarshalHelper.GetString(pActualLang, 0, actualLangLength, Encoding.UTF8);
						Common_FreeString(pActualLang);
					}
					else
					{
						actualLang = string.Empty;
					}

					if (itemValueLength > 0 && pItemValue != IntPtr.Zero)
					{
						itemValue = MarshalHelper.GetString(pItemValue, 0, itemValueLength, Encoding.UTF8);
						Common_FreeString(pItemValue);
					}
					else
					{
						itemValue = string.Empty;
					}
				}
				else
				{
					actualLang = null;
					itemValue = null;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pAltTextName);
				MarshalHelper.FreeString(pGenericLang);
				MarshalHelper.FreeString(pSpecificLang);
			}
			return result;
		}

		public void SetLocalizedText(string schemaNS, string altTextName, string genericLang, string specificLang, string itemValue, int options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pAltTextName = IntPtr.Zero;
			IntPtr pGenericLang = IntPtr.Zero;
			IntPtr pSpecificLang = IntPtr.Zero;
			IntPtr pItemValue = IntPtr.Zero;

			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pAltTextName = MarshalHelper.GetString(altTextName, Encoding.UTF8);
				pGenericLang = MarshalHelper.GetString(genericLang, Encoding.UTF8);
				pSpecificLang = MarshalHelper.GetString(specificLang, Encoding.UTF8);
				pItemValue = MarshalHelper.GetString(itemValue, Encoding.UTF8);
				XMPMeta_SetLocalizedText(xmpCoreHandle, pSchemaNS, pAltTextName, pGenericLang, pSpecificLang, pItemValue, options);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pAltTextName);
				MarshalHelper.FreeString(pGenericLang);
				MarshalHelper.FreeString(pSpecificLang);
				MarshalHelper.FreeString(pItemValue);
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetLocalizedText", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_GetLocalizedText(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr altTextName, IntPtr genericLang, IntPtr specificLang, out IntPtr actualLang, out int actualLangLength, out IntPtr itemValue, out int itemValueLength, out int options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_SetLocalizedText", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_SetLocalizedText(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr altTextName, IntPtr genericLang, IntPtr specificLang, IntPtr itemValue, int options);

		#endregion

		#region Functions accessing properties as binary values

		public bool GetProperty(string schemaNS, string propName, out bool propValue, out PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				result = XMPMeta_GetProperty_Bool(xmpCoreHandle, pSchemaNS, pPropName, out propValue, out options);
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
			return result; 
		}

		public bool GetProperty(string schemaNS, string propName, out int propValue, out PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				result = XMPMeta_GetProperty_Int(xmpCoreHandle, pSchemaNS, pPropName, out propValue, out options);
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
			return result;
		}

		public bool GetProperty(string schemaNS, string propName, out long propValue, out PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				result = XMPMeta_GetProperty_Int64(xmpCoreHandle, pSchemaNS, pPropName, out propValue, out options);
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
			return result;
		}

		public bool GetProperty(string schemaNS, string propName, out double propValue, out PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				result = XMPMeta_GetProperty_Float(xmpCoreHandle, pSchemaNS, pPropName, out propValue, out options);
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
			return result; 
		}

		public bool GetProperty(string schemaNS, string propName, out DateTime propValue, out PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;
			PInvoke.XmpDateTime pPropValue = new PInvoke.XmpDateTime();
			bool result = false;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				if (XMPMeta_GetProperty_Date(xmpCoreHandle, pSchemaNS, pPropName, ref pPropValue, out options))
				{
					propValue = XmpDateTime.XmpDateTimeToDateTime(pPropValue);
					result = true;
				}
				else
				{
					propValue = DateTime.MinValue;
					result = false;
				}
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
			return result; 
		}

		public void SetProperty(string schemaNS, string propName, bool propValue, PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;

			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				XMPMeta_SetProperty_Bool(xmpCoreHandle, pSchemaNS, pPropName, propValue, options);
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

		public void SetProperty(string schemaNS, string propName, int propValue, PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;

			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				XMPMeta_SetProperty_Int(xmpCoreHandle, pSchemaNS, pPropName, propValue, options);
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

		public void SetProperty(string schemaNS, string propName, long propValue, PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;

			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				XMPMeta_SetProperty_Int64(xmpCoreHandle, pSchemaNS, pPropName, propValue, options);
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

		public void SetProperty(string schemaNS, string propName, double propValue, PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;

			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				XMPMeta_SetProperty_Float(xmpCoreHandle, pSchemaNS, pPropName, propValue, options);
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

		public void SetProperty(string schemaNS, string propName, DateTime propValue, PropertyFlags options)
		{
			AssertValidState();

			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;

			PInvoke.XmpDateTime pPropValue = XmpDateTime.DateTimeToXmpDateTime(propValue, TimeZone.CurrentTimeZone);

			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				XMPMeta_SetProperty_Date(xmpCoreHandle, pSchemaNS, pPropName, pPropValue, options);
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

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetProperty_Bool", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_GetProperty_Bool(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, out bool propValue, out PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetProperty_Int", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_GetProperty_Int(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, out int propValue, out PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetProperty_Int64", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_GetProperty_Int64(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, out long propValue, out PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetProperty_Float", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_GetProperty_Float(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, out double propValue, out PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetProperty_Date", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_GetProperty_Date(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, ref PInvoke.XmpDateTime propValue, out PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_SetProperty_Bool", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_SetProperty_Bool(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, bool propValue, PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_SetProperty_Int", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_SetProperty_Int(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, int propValue, PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_SetProperty_Int64", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_SetProperty_Int64(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, long propValue, PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_SetProperty_Float", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_SetProperty_Float(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, double propValue, PropertyFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_SetProperty_Date", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_SetProperty_Date(IntPtr xmpCoreHandle, IntPtr schemaNS, IntPtr propName, PInvoke.XmpDateTime propValue, PropertyFlags options);

		#endregion

		#region Misceallaneous functions

		public int DumpObject(TextOutputDelegate outProc, object state)
		{
			AssertValidState();
			if (outProc != null)
			{
				DumpObjectStruct dumpObject = new DumpObjectStruct();
				dumpObject.TextOutput = outProc;
				dumpObject.State = state;

				IntPtr refCon = Marshal.AllocHGlobal(Marshal.SizeOf(dumpObject));
				int result;
				try
				{
					Marshal.StructureToPtr(dumpObject, refCon, false);
					result = XMPMeta_DumpObject(xmpCoreHandle, new PInvoke.TextOutputDelegate(DumpObjectTextOutput), refCon);
				}
				catch
				{
					result = -1;
				}
				finally
				{
					Marshal.DestroyStructure(refCon, typeof(DumpObjectStruct));
					Marshal.FreeHGlobal(refCon);
				}
				return result;
			}
			else
			{
				return -1;
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_DumpObject", CharSet = CharSet.Auto)]
		private static extern int XMPMeta_DumpObject(IntPtr xmpCoreHandle, PInvoke.TextOutputDelegate outProc, IntPtr refCon);

		#endregion

		#region Functions for parsing and serializing

		public void ParseFromBuffer(string buffer, ParseFlags options)
		{
			AssertValidState();

			IntPtr pBuffer = IntPtr.Zero;

			try
			{
				int bufferLength;
				pBuffer = MarshalHelper.GetString(buffer, Encoding.UTF8, out bufferLength);
				XMPMeta_ParseFromBuffer(xmpCoreHandle, pBuffer, bufferLength, options);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pBuffer);
			}
		}

		public void SerializeToBuffer(out string rdfString, SerializeFlags options, int padding, string newline, string indent, int baseIndent)
		{
			AssertValidState();

			IntPtr pNewline = IntPtr.Zero;
			IntPtr pIndent = IntPtr.Zero;

			IntPtr pRdfString;
			int rdfStringLength;

			try
			{
				pNewline = MarshalHelper.GetString(newline, Encoding.UTF8);
				pIndent = MarshalHelper.GetString(indent, Encoding.UTF8);
				XMPMeta_SerializeToBuffer1(xmpCoreHandle, out pRdfString, out rdfStringLength, options, padding, pNewline, pIndent, baseIndent);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pNewline);
				MarshalHelper.FreeString(pIndent);
			}

			if (rdfStringLength > 0 && pRdfString != IntPtr.Zero)
			{
				rdfString = MarshalHelper.GetString(pRdfString, 0, rdfStringLength, Encoding.UTF8);
				Common_FreeString(pRdfString);
			}
			else
			{
				rdfString = string.Empty;
			}
		}

		public void SerializeToBuffer(out string rdfString, SerializeFlags options, int padding)
		{
			AssertValidState();

			IntPtr pRdfString;
			int rdfStringLength;

			XMPMeta_SerializeToBuffer2(xmpCoreHandle, out pRdfString, out rdfStringLength, options, padding);

			if (rdfStringLength > 0 && pRdfString != IntPtr.Zero)
			{
				rdfString = MarshalHelper.GetString(pRdfString, 0, rdfStringLength, Encoding.UTF8);
				Common_FreeString(pRdfString);
			}
			else
			{
				rdfString = string.Empty;
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_ParseFromBuffer", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_ParseFromBuffer(IntPtr xmpCoreHandle, IntPtr buffer, int bufferSize, ParseFlags options);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_SerializeToBuffer1", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_SerializeToBuffer1(IntPtr xmpCoreHandle, out IntPtr rdfString, out int rdfStringLength, SerializeFlags options, int padding, IntPtr newline, IntPtr indent, int baseIndent);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_SerializeToBuffer2", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_SerializeToBuffer2(IntPtr xmpCoreHandle, out IntPtr rdfString, out int rdfStringLength, SerializeFlags options, int padding);

		#endregion

		#endregion

		#region Static Public Member Functions

		#region Initialization and termination

		public static XmpVersionInfo GetVersionInfo()
		{
			PInvoke.XmpVersionInfo xmpVersionInfo = new PInvoke.XmpVersionInfo();
			XMPMeta_GetVersionInfo(ref xmpVersionInfo);
			return new XmpVersionInfo(xmpVersionInfo);
		}

		public static bool Initialize()
		{
			if (initialized)
			{
				return true;
			}
			else
			{
				initialized = XMPMeta_Initialize();
				return initialized;
			}
		}

		public static void Terminate()
		{
			XMPMeta_Terminate();
			initialized = false;
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetVersionInfo", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_GetVersionInfo(ref PInvoke.XmpVersionInfo xmpVersionInfo);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_Initialize", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_Initialize();

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_Terminate", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_Terminate();

		#endregion

		#region Global option flags

		public static int GetGlobalOptions()
		{
			return XMPMeta_GetGlobalOptions();
		}

		public static void SetGlobalOptions(int options)
		{
			XMPMeta_SetGlobalOptions(options);
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetGlobalOptions", CharSet = CharSet.Auto)]
		private static extern int XMPMeta_GetGlobalOptions();

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_SetGlobalOptions", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_SetGlobalOptions(int options);

		#endregion

		#region Internal data structure dump utilities

		public static int DumpNamespaces(TextOutputDelegate outProc, object state)
		{
			if (outProc != null)
			{
				DumpObjectStruct dumpObject = new DumpObjectStruct();
				dumpObject.TextOutput = outProc;
				dumpObject.State = state;

				IntPtr refCon = Marshal.AllocHGlobal(Marshal.SizeOf(dumpObject));
				int result;
				try
				{
					Marshal.StructureToPtr(dumpObject, refCon, false);
					result = XMPMeta_DumpNamespaces(new PInvoke.TextOutputDelegate(DumpObjectTextOutput), refCon);
				}
				catch
				{
					result = -1;
				}
				finally
				{
					Marshal.DestroyStructure(refCon, typeof(DumpObjectStruct));
					Marshal.FreeHGlobal(refCon);
				}
				return result;
			}
			else
			{
				return -1;
			}
		}

		public static int DumpAliases(TextOutputDelegate outProc, object state)
		{
			if (outProc != null)
			{
				DumpObjectStruct dumpObject = new DumpObjectStruct();
				dumpObject.TextOutput = outProc;
				dumpObject.State = state;

				IntPtr refCon = Marshal.AllocHGlobal(Marshal.SizeOf(dumpObject));
				int result;
				try
				{
					Marshal.StructureToPtr(dumpObject, refCon, false);
					result = XMPMeta_DumpAliases(new PInvoke.TextOutputDelegate(DumpObjectTextOutput), refCon);
				}
				catch
				{
					result = -1;
				}
				finally
				{
					Marshal.DestroyStructure(refCon, typeof(DumpObjectStruct));
					Marshal.FreeHGlobal(refCon);
				}
				return result;
			}
			else
			{
				return -1;
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_DumpNamespaces", CharSet = CharSet.Auto)]
		private static extern int XMPMeta_DumpNamespaces(PInvoke.TextOutputDelegate outProc, IntPtr refCon);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_DumpAliases", CharSet = CharSet.Auto)]
		private static extern int XMPMeta_DumpAliases(PInvoke.TextOutputDelegate outProc, IntPtr refCon);

		#endregion

		#region Namespace Functions

		private static bool GetStringFromPtr(IntPtr ptr, int length, out string value)
		{
			if (length > 0 && ptr != IntPtr.Zero)
			{
				value = MarshalHelper.GetString(ptr, 0, length, Encoding.UTF8);
				Common_FreeString(ptr);
				return true;
			}
			else
			{
				value = null;
				return false;
			}
		}

		public static bool RegisterNamespace(string namespaceURI, string suggestedPrefix, out string registeredPrefix)
		{
			bool result = false;

			IntPtr pNamespaceURI = IntPtr.Zero;
			IntPtr pSuggestedPrefix = IntPtr.Zero;

			IntPtr pRegisteredPrefix;
			int registeredPrefixLength;

			try
			{
				pNamespaceURI = MarshalHelper.GetString(namespaceURI, Encoding.UTF8);
				pSuggestedPrefix = MarshalHelper.GetString(suggestedPrefix, Encoding.UTF8);
				if (XMPMeta_RegisterNamespace(pNamespaceURI, pSuggestedPrefix, out pRegisteredPrefix, out registeredPrefixLength))
				{
					if (GetStringFromPtr(pRegisteredPrefix, registeredPrefixLength, out registeredPrefix))
					{
						result = true;
					}
					else
					{
						// RegisterNamespace should *always* return a prefix, whether it succeeds or not.
						throw new InvalidOperationException("Exception in XMPCore");
					}
				}
				else
				{
					registeredPrefix = null;
					result = false;
				}
			}
			finally
			{
				MarshalHelper.FreeString(pNamespaceURI);
				MarshalHelper.FreeString(pSuggestedPrefix);
			}

			return result;
		}

		public static bool GetNamespacePrefix(string namespaceURI, out string namespacePrefix)
		{
			IntPtr pNamespacePrefix;
			int namespacePrefixLength;

			IntPtr pNamespaceURI = IntPtr.Zero;
			bool result = false;
			try
			{
				pNamespaceURI = MarshalHelper.GetString(namespaceURI, Encoding.UTF8);
				if (XMPMeta_GetNamespacePrefix(pNamespaceURI, out pNamespacePrefix, out namespacePrefixLength))
				{
					if (namespacePrefixLength > 0 && pNamespacePrefix != IntPtr.Zero)
					{
						namespacePrefix = MarshalHelper.GetString(pNamespacePrefix, 0, namespacePrefixLength, Encoding.UTF8);
						Common_FreeString(pNamespacePrefix);
					}
					else
					{
						namespacePrefix = string.Empty;
					}
					result = true;
				}
				else
				{
					namespacePrefix = null;
					result = false;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pNamespaceURI);
			}
			return result;
		}

		public static bool GetNamespaceURI(string namespacePrefix, out string namespaceURI)
		{
			IntPtr pNamespaceURI;
			int namespaceURILength;

			IntPtr pNamespacePrefix = IntPtr.Zero;
			bool result = false;
			try
			{
				pNamespacePrefix = MarshalHelper.GetString(namespacePrefix, Encoding.UTF8);
				if (XMPMeta_GetNamespaceURI(pNamespacePrefix, out pNamespaceURI, out namespaceURILength))
				{
					if (namespaceURILength > 0 && pNamespaceURI != IntPtr.Zero)
					{
						namespaceURI = MarshalHelper.GetString(pNamespaceURI, 0, namespaceURILength, Encoding.UTF8);
						Common_FreeString(pNamespaceURI);
					}
					else
					{
						namespaceURI = string.Empty;
					}
					result = true;
				}
				else
				{
					namespaceURI = null;
					result = false;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pNamespacePrefix);
			}
			return result;
		}

		public static void DeleteNamespace(string namespaceURI)
		{
			throw new NotImplementedException("Not yet implemented (XMPCore 4.1.1)");
			//XMPMeta_DeleteNamespace(namespaceURI);
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_RegisterNamespace", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_RegisterNamespace(IntPtr namespaceURI, IntPtr suggestedPrefix, out IntPtr registeredPrefix, out int registeredPrefixLength);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetNamespacePrefix", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_GetNamespacePrefix(IntPtr namespaceURI, out IntPtr namespacePrefix, out int namespacePrefixLength);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_GetNamespaceURI", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_GetNamespaceURI(IntPtr namespacePrefix, out IntPtr namespaceURI, out int namespaceURILength);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_DeleteNamespace", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_DeleteNamespace(IntPtr namespaceURI);

		#endregion

		#region Alias Functions

		public static void RegisterAlias(string aliasNS, string aliasProp, string actualNS, string actualProp, PropertyFlags arrayForm)
		{
			IntPtr pAliasNS = IntPtr.Zero;
			IntPtr pAliasProp = IntPtr.Zero;
			IntPtr pActualNS = IntPtr.Zero;
			IntPtr pActualProp = IntPtr.Zero;

			try
			{
				pAliasNS = MarshalHelper.GetString(aliasNS, Encoding.UTF8);
				pAliasProp = MarshalHelper.GetString(aliasProp, Encoding.UTF8);
				pActualNS = MarshalHelper.GetString(actualNS, Encoding.UTF8);
				pActualProp = MarshalHelper.GetString(actualProp, Encoding.UTF8);
				XMPMeta_RegisterAlias(pAliasNS, pAliasProp, pActualNS, pActualProp, arrayForm);
			}
			catch (XmpException ex)
			{
				throw ex;
			}
			finally
			{
				MarshalHelper.FreeString(pAliasNS);
				MarshalHelper.FreeString(pAliasProp);
				MarshalHelper.FreeString(pActualNS);
				MarshalHelper.FreeString(pActualProp);
			}
		}

		public static bool ResolveAlias(string aliasNS, string aliasProp, out string actualNS, out string actualProp, out PropertyFlags arrayForm)
		{
			IntPtr pActualNS;
			int actualNSLength;

			IntPtr pActualProp;
			int actualPropLength;

			IntPtr pAliasNS = IntPtr.Zero;
			IntPtr pAliasProp = IntPtr.Zero;
			bool result = false;
			try
			{
				pAliasNS = MarshalHelper.GetString(aliasNS, Encoding.UTF8);
				pAliasProp = MarshalHelper.GetString(aliasProp, Encoding.UTF8);
				if (XMPMeta_ResolveAlias(pAliasNS, pAliasProp, out pActualNS, out actualNSLength, out pActualProp, out actualPropLength, out arrayForm))
				{
					if (actualNSLength > 0 && pActualNS != IntPtr.Zero)
					{
						actualNS = MarshalHelper.GetString(pActualNS, 0, actualNSLength, Encoding.UTF8);
						Common_FreeString(pActualNS);
					}
					else
					{
						actualNS = string.Empty;
					}

					if (actualPropLength > 0 && pActualProp != IntPtr.Zero)
					{
						actualProp = MarshalHelper.GetString(pActualProp, 0, actualPropLength, Encoding.UTF8);
						Common_FreeString(pActualProp);
					}
					else
					{
						actualProp = string.Empty;
					}

					result = true;
				}
				else
				{
					actualNS = null;
					actualProp = null;
					result = false;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pAliasNS);
				MarshalHelper.FreeString(pAliasProp);
			}
			return result;
		}

		public static void DeleteAlias(string aliasNS, string aliasProp)
		{
			IntPtr pAliasNS = IntPtr.Zero;
			IntPtr pAliasProp = IntPtr.Zero;

			try
			{
				pAliasNS = MarshalHelper.GetString(aliasNS, Encoding.UTF8);
				pAliasProp = MarshalHelper.GetString(aliasProp, Encoding.UTF8);
				XMPMeta_DeleteAlias(pAliasNS, pAliasProp);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pAliasNS);
				MarshalHelper.FreeString(pAliasProp);
			}
		}

		public static void RegisterStandardAliased(string schemaNS)
		{
			IntPtr pSchemaNS = IntPtr.Zero;

			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				XMPMeta_RegisterStandardAliases(pSchemaNS);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_RegisterAlias", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_RegisterAlias(IntPtr aliasNS, IntPtr aliasProp, IntPtr actualNS, IntPtr actualProp, PropertyFlags arrayForm);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_ResolveAlias", CharSet = CharSet.Auto)]
		private static extern bool XMPMeta_ResolveAlias(IntPtr aliasNS, IntPtr aliasProp, out IntPtr actualNS, out int actualNSLength, out IntPtr actualProp, out int actualPropLength, out PropertyFlags arrayForm);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_DeleteAlias", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_DeleteAlias(IntPtr aliasNS, IntPtr aliasProp);

		[DllImport("XmpToolkit", EntryPoint = "XMPMeta_RegisterStandardAliases", CharSet = CharSet.Auto)]
		private static extern void XMPMeta_RegisterStandardAliases(IntPtr schemaNS);

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
				throw new InvalidOperationException("Initialize must be called before using an XmpCore object.");
			}
			if (xmpCoreHandle == IntPtr.Zero)
			{
				throw new NullReferenceException("XmpCore is null.");
			}
		}

		private struct DumpObjectStruct
		{
			public TextOutputDelegate TextOutput;
			public object State;
		}

		private static int DumpObjectTextOutput(IntPtr refCon, IntPtr buffer, int bufferSize)
		{
			DumpObjectStruct dumpObject = (DumpObjectStruct)Marshal.PtrToStructure(refCon, typeof(DumpObjectStruct));
			string textOutput = MarshalHelper.GetString(buffer, 0, bufferSize, Encoding.UTF8);
			int result;
			try
			{
				dumpObject.TextOutput(dumpObject.State, textOutput);
				result = 0;
			}
			catch
			{
				result = -1;
			}
			return result;
		}

		[DllImport("XmpToolkit", EntryPoint = "Common_FreeString", CharSet = CharSet.Auto)]
		private static extern void Common_FreeString(IntPtr stringHandle);

		[DllImport("XmpToolkit", EntryPoint = "Common_GetLastError", CharSet = CharSet.Auto)]
		private static extern int Common_GetLastError();

		#endregion
	}
}
