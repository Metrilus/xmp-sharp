using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// XMP Toolkit utility services.
	/// </summary>
	public static class XmpUtils
	{
		#region Path composition functions

		public static void ComposeArrayItemPath(string schemaNS, string arrayName, int itemIndex, out string fullPath)
		{
			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pArrayName = IntPtr.Zero;

			IntPtr pFullPath;
			int fullPathLength;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pArrayName = MarshalHelper.GetString(arrayName, Encoding.UTF8);
				XMPUtils_ComposeArrayItemPath(pSchemaNS, pArrayName, itemIndex, out pFullPath, out fullPathLength);

				if (fullPathLength > 0 && pFullPath != IntPtr.Zero)
				{
					fullPath = MarshalHelper.GetString(pFullPath, 0, fullPathLength, Encoding.UTF8);
					Common_FreeString(pFullPath);
				}
				else
				{
					fullPath = null;
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
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="schemaNS"></param>
		/// <param name="structName"></param>
		/// <param name="fieldNS"></param>
		/// <param name="fieldName"></param>
		/// <param name="fullPath"></param>
		public static void ComposeStructFieldPath(string schemaNS, string structName, string fieldNS, string fieldName, out string fullPath)
		{
			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pStructName = IntPtr.Zero;
			IntPtr pFieldNS = IntPtr.Zero;
			IntPtr pFieldName = IntPtr.Zero;

			IntPtr pFullPath;
			int fullPathLength;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pStructName = MarshalHelper.GetString(structName, Encoding.UTF8);
				pFieldNS = MarshalHelper.GetString(fieldNS, Encoding.UTF8);
				pFieldName = MarshalHelper.GetString(fieldName, Encoding.UTF8);
				XMPUtils_ComposeStructFieldPath(pSchemaNS, pStructName, pFieldNS, pFieldName, out pFullPath, out fullPathLength);

				if (fullPathLength > 0 && pFullPath != IntPtr.Zero)
				{
					fullPath = MarshalHelper.GetString(pFullPath, 0, fullPathLength, Encoding.UTF8);
					Common_FreeString(pFullPath);
				}
				else
				{
					fullPath = null;
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
		}

		public static void ComposeQualifierPath(string schemaNS, string propName, string qualNS, string qualName, out string fullPath)
		{
			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pPropName = IntPtr.Zero;
			IntPtr pQualNS = IntPtr.Zero;
			IntPtr pQualName = IntPtr.Zero;

			IntPtr pFullPath;
			int fullPathLength;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pPropName = MarshalHelper.GetString(propName, Encoding.UTF8);
				pQualNS = MarshalHelper.GetString(qualNS, Encoding.UTF8);
				pQualName = MarshalHelper.GetString(qualName, Encoding.UTF8);
				XMPUtils_ComposeQualifierPath(pSchemaNS, pPropName, pQualNS, pQualName, out pFullPath, out fullPathLength);

				if (fullPathLength > 0 && pFullPath != IntPtr.Zero)
				{
					fullPath = MarshalHelper.GetString(pFullPath, 0, fullPathLength, Encoding.UTF8);
					Common_FreeString(pFullPath);
				}
				else
				{
					fullPath = null;
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
		}

		public static void ComposeLangSelectorComposeQualifierPath(string schemaNS, string arrayName, string langName, out string fullPath)
		{
			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pArrayName = IntPtr.Zero;
			IntPtr pLangName = IntPtr.Zero;

			IntPtr pFullPath;
			int fullPathLength;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pArrayName = MarshalHelper.GetString(arrayName, Encoding.UTF8);
				pLangName = MarshalHelper.GetString(langName, Encoding.UTF8);
				XMPUtils_ComposeLangSelector(pSchemaNS, pArrayName, pLangName, out pFullPath, out fullPathLength);

				if (fullPathLength > 0 && pFullPath != IntPtr.Zero)
				{
					fullPath = MarshalHelper.GetString(pFullPath, 0, fullPathLength, Encoding.UTF8);
					Common_FreeString(pFullPath);
				}
				else
				{
					fullPath = null;
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
				MarshalHelper.FreeString(pLangName);
			}
		}

		public static void ComposeFieldSelector(string schemaNS, string arrayName, string fieldNS, string fieldName, string fieldValue, out string fullPath)
		{
			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pArrayName = IntPtr.Zero;
			IntPtr pFieldNS = IntPtr.Zero;
			IntPtr pFieldName = IntPtr.Zero;
			IntPtr pFieldValue = IntPtr.Zero;

			IntPtr pFullPath;
			int fullPathLength;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pArrayName = MarshalHelper.GetString(arrayName, Encoding.UTF8);
				pFieldNS = MarshalHelper.GetString(fieldNS, Encoding.UTF8);
				pFieldName = MarshalHelper.GetString(fieldName, Encoding.UTF8);
				pFieldValue = MarshalHelper.GetString(fieldValue, Encoding.UTF8);
				XMPUtils_ComposeFieldSelector(pSchemaNS, pArrayName, pFieldNS, pFieldName, pFieldValue, out pFullPath, out fullPathLength);

				if (fullPathLength > 0 && pFullPath != IntPtr.Zero)
				{
					fullPath = MarshalHelper.GetString(pFullPath, 0, fullPathLength, Encoding.UTF8);
					Common_FreeString(pFullPath);
				}
				else
				{
					fullPath = null;
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
				MarshalHelper.FreeString(pFieldNS);
				MarshalHelper.FreeString(pFieldName);
				MarshalHelper.FreeString(pFieldValue);
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ComposeArrayItemPath", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_ComposeArrayItemPath(IntPtr schemaNS, IntPtr arrayName, int itemIndex, out IntPtr fullPath, out int fullPathLength);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ComposeStructFieldPath", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_ComposeStructFieldPath(IntPtr schemaNS, IntPtr structName, IntPtr fieldNS, IntPtr fieldName, out IntPtr fullPath, out int fullPathLength);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ComposeQualifierPath", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_ComposeQualifierPath(IntPtr schemaNS, IntPtr propName, IntPtr qualNS, IntPtr qualName, out IntPtr fullPath, out int fullPathLength);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ComposeLangSelector", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_ComposeLangSelector(IntPtr schemaNS, IntPtr arrayName, IntPtr langName, out IntPtr fullPath, out int fullPathLength);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ComposeFieldSelector", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_ComposeFieldSelector(IntPtr schemaNS, IntPtr arrayName, IntPtr fieldNS, IntPtr fieldName, IntPtr fieldValue, out IntPtr fullPath, out int fullPathLength);

		#endregion

		#region Binary-String conversion functions

		public static void ConvertFromBool(bool binValue, out string strValue)
		{
			IntPtr pStrValue = IntPtr.Zero;
			int strValueLength;
			try
			{
				XMPUtils_ConvertFromBool(binValue, out pStrValue, out strValueLength);
				if (strValueLength > 0 && pStrValue != IntPtr.Zero)
				{
					strValue = MarshalHelper.GetString(pStrValue, 0, strValueLength, Encoding.UTF8);
				}
				else
				{
					strValue = null;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				if (pStrValue != IntPtr.Zero)
				{
					Common_FreeString(pStrValue);
				}
			}
		}

		public static void ConvertFromInt(long binValue, string format, out string strValue)
		{
			IntPtr pFormat = IntPtr.Zero;

			IntPtr pStrValue = IntPtr.Zero;
			int strValueLength;
			try
			{
				if (format != null && format.Length > 0)
				{
					pFormat = MarshalHelper.GetString(format, Encoding.UTF8);
				}
				XMPUtils_ConvertFromInt(binValue, pFormat, out pStrValue, out strValueLength);
				if (strValueLength > 0 && pStrValue != IntPtr.Zero)
				{
					strValue = MarshalHelper.GetString(pStrValue, 0, strValueLength, Encoding.UTF8);
				}
				else
				{
					strValue = null;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				if (pStrValue != IntPtr.Zero)
				{
					Common_FreeString(pStrValue);
				}
				if (pFormat != IntPtr.Zero)
				{
					MarshalHelper.FreeString(pFormat);
				}
			}
		}

		public static void ConvertFromFloat(double binValue, string format, out string strValue)
		{
			IntPtr pFormat = IntPtr.Zero;

			IntPtr pStrValue = IntPtr.Zero;
			int strValueLength;
			try
			{
				if (format != null && format.Length > 0)
				{
					pFormat = MarshalHelper.GetString(format, Encoding.UTF8);
				}
				XMPUtils_ConvertFromFloat(binValue, pFormat, out pStrValue, out strValueLength);
				if (strValueLength > 0 && pStrValue != IntPtr.Zero)
				{
					strValue = MarshalHelper.GetString(pStrValue, 0, strValueLength, Encoding.UTF8);
				}
				else
				{
					strValue = null;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				if (pStrValue != IntPtr.Zero)
				{
					Common_FreeString(pStrValue);
				}
				if (pFormat != IntPtr.Zero)
				{
					MarshalHelper.FreeString(pFormat);
				}
			}
		}

		public static void ConvertFromDate(DateTime binValue, out string strValue)
		{
			strValue = XmpDateTime.DateTimeToXmpString(binValue, TimeZone.CurrentTimeZone);
		}

		internal static void ConvertFromDate(PInvoke.XmpDateTime binValue, out string strValue)
		{
			IntPtr pStrValue = IntPtr.Zero;
			int strValueLength;
			try
			{
				XMPUtils_ConvertFromDate(binValue, out pStrValue, out strValueLength);
				if (strValueLength > 0 && pStrValue != IntPtr.Zero)
				{
					strValue = MarshalHelper.GetString(pStrValue, 0, strValueLength, Encoding.UTF8);
				}
				else
				{
					strValue = null;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				if (pStrValue != IntPtr.Zero)
				{
					Common_FreeString(pStrValue);
				}
			}
		}

		public static bool ConvertToBool(string strValue)
		{
			bool result;
			IntPtr pStrValue = IntPtr.Zero;
			try
			{
				pStrValue = MarshalHelper.GetString(strValue, Encoding.UTF8);
				result = XMPUtils_ConvertToBool(pStrValue);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pStrValue);
			}
			return result;
		}

		public static int ConvertToInt(string strValue)
		{
			int result;
			IntPtr pStrValue = IntPtr.Zero;
			try
			{
				pStrValue = MarshalHelper.GetString(strValue, Encoding.UTF8);
				result = XMPUtils_ConvertToInt(pStrValue);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pStrValue);
			}
			return result;
		}

		public static long ConvertToInt64(string strValue)
		{
			long result;
			IntPtr pStrValue = IntPtr.Zero;
			try
			{
				pStrValue = MarshalHelper.GetString(strValue, Encoding.UTF8);
				result = XMPUtils_ConvertToInt64(pStrValue);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pStrValue);
			}
			return result;
		}

		public static double ConvertToFloat(string strValue)
		{
			double result;
			IntPtr pStrValue = IntPtr.Zero;
			try
			{
				pStrValue = MarshalHelper.GetString(strValue, Encoding.UTF8);
				result = XMPUtils_ConvertToFloat(pStrValue);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pStrValue);
			}
			return result;
		}

		public static void ConvertToDate(string strValue, out DateTime binValue)
		{
			binValue = XmpDateTime.XmpStringToDateTime(strValue);
		}

		internal static void ConvertToDate(string strValue, out PInvoke.XmpDateTime binValue)
		{
			IntPtr pStrValue = IntPtr.Zero;
			try
			{
				pStrValue = MarshalHelper.GetString(strValue, Encoding.UTF8);
				XMPUtils_ConvertToDate(pStrValue, out binValue);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pStrValue);
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ConvertFromBool", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_ConvertFromBool(bool binValue, out IntPtr strValue, out int strValueLength);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ConvertFromInt", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_ConvertFromInt(long binValue, IntPtr format, out IntPtr strValue, out int strValueLength);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ConvertFromFloat", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_ConvertFromFloat(double binValue, IntPtr format, out IntPtr strValue, out int strValueLength);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ConvertFromDate", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_ConvertFromDate(PInvoke.XmpDateTime binValue, out IntPtr strValue, out int strValueLength);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ConvertToBool", CharSet = CharSet.Auto)]
		private static extern bool XMPUtils_ConvertToBool(IntPtr strValue);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ConvertToInt", CharSet = CharSet.Auto)]
		private static extern int XMPUtils_ConvertToInt(IntPtr strValue);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ConvertToInt64", CharSet = CharSet.Auto)]
		private static extern long XMPUtils_ConvertToInt64(IntPtr strValue);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ConvertToFloat", CharSet = CharSet.Auto)]
		private static extern double XMPUtils_ConvertToFloat(IntPtr strValue);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ConvertToDate", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_ConvertToDate(IntPtr strValue, out PInvoke.XmpDateTime binValue);

		#endregion

		#region Date/Time functions

		public static void CurrentDateTime(out DateTime time)
		{
			try
			{
				PInvoke.XmpDateTime xmpDateTime;
				XMPUtils_CurrentDateTime(out xmpDateTime);
				time = XmpDateTime.XmpDateTimeToDateTime(xmpDateTime);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
		}

		public static void SetTimeZone(ref DateTime time)
		{
			try
			{
				PInvoke.XmpDateTime xmpDateTime = XmpDateTime.DateTimeToXmpDateTime(time, TimeZone.CurrentTimeZone);
				XMPUtils_SetTimeZone(ref xmpDateTime);
				time = XmpDateTime.XmpDateTimeToDateTime(xmpDateTime);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
		}

		public static void ConvertToUTCTime(ref DateTime time)
		{
			try
			{
				PInvoke.XmpDateTime xmpDateTime = XmpDateTime.DateTimeToXmpDateTime(time, TimeZone.CurrentTimeZone);
				XMPUtils_ConvertToUTCTime(ref xmpDateTime);
				time = XmpDateTime.XmpDateTimeToDateTime(xmpDateTime);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
		}

		public static void ConvertToLocalTime(ref DateTime time)
		{
			try
			{
				PInvoke.XmpDateTime xmpDateTime = XmpDateTime.DateTimeToXmpDateTime(time, TimeZone.CurrentTimeZone);
				XMPUtils_ConvertToLocalTime(ref xmpDateTime);
				time = XmpDateTime.XmpDateTimeToDateTime(xmpDateTime);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
		}

		public static int CompareDateTime(DateTime left, DateTime right)
		{
			try
			{
				PInvoke.XmpDateTime xmpDateTime1 = XmpDateTime.DateTimeToXmpDateTime(left, TimeZone.CurrentTimeZone);
				PInvoke.XmpDateTime xmpDateTime2 = XmpDateTime.DateTimeToXmpDateTime(right, TimeZone.CurrentTimeZone);
				return XMPUtils_CompareDateTime(xmpDateTime1, xmpDateTime2);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_CurrentDateTime", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_CurrentDateTime(out PInvoke.XmpDateTime time);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_SetTimeZone", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_SetTimeZone(ref PInvoke.XmpDateTime time);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ConvertToUTCTime", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_ConvertToUTCTime(ref PInvoke.XmpDateTime time);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_ConvertToLocalTime", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_ConvertToLocalTime(ref PInvoke.XmpDateTime time);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_CompareDateTime", CharSet = CharSet.Auto)]
		private static extern int XMPUtils_CompareDateTime(PInvoke.XmpDateTime left, PInvoke.XmpDateTime right);

		#endregion

		#region Base 64 Encoding and Decoding

		public static void EncodeToBase64(string rawStr, out string encodedStr)
		{
			IntPtr pRawStr = IntPtr.Zero;

			IntPtr pEncodedStr;
			int encodedStrLength;
			try
			{
				int rawStrSize;
				pRawStr = MarshalHelper.GetString(rawStr, Encoding.UTF8, out rawStrSize);
				XMPUtils_EncodeToBase64(pRawStr, rawStrSize, out pEncodedStr, out encodedStrLength);

				if (encodedStrLength > 0 && pEncodedStr != IntPtr.Zero)
				{
					encodedStr = MarshalHelper.GetString(pEncodedStr, 0, encodedStrLength, Encoding.UTF8);
					Common_FreeString(pEncodedStr);
				}
				else
				{
					encodedStr = null;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pRawStr);
			}
		}

		public static void DecodeFromBase64(string encodedStr, out string rawStr)
		{
			IntPtr pEncodedStr = IntPtr.Zero;

			IntPtr pRawStr;
			int rawStrLength;
			try
			{
				int encodedStrSize;
				pEncodedStr = MarshalHelper.GetString(encodedStr, Encoding.UTF8, out encodedStrSize);
				XMPUtils_DecodeFromBase64(pEncodedStr, encodedStrSize, out pRawStr, out rawStrLength);

				if (rawStrLength > 0 && pRawStr != IntPtr.Zero)
				{
					rawStr = MarshalHelper.GetString(pRawStr, 0, rawStrLength, Encoding.UTF8);
					Common_FreeString(pRawStr);
				}
				else
				{
					rawStr = null;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pEncodedStr);
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_EncodeToBase64", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_EncodeToBase64(IntPtr rawStr, int rawLen, out IntPtr encodedStr, out int encodedStrLength);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_DecodeFromBase64", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_DecodeFromBase64(IntPtr encodedStr, int encodedLen, out IntPtr rawStr, out int rawStrLength);

		#endregion

		#region JPEG file handling

		public static void PackageForJPEG(XmpCore xmpCore, out string standardXmp, out string extendedXmp, out string extendedDigest)
		{
			IntPtr pStandardXmp;
			IntPtr pExtendedXmp;
			IntPtr pExtendedDigest;
			int standardXmpLength;
			int extendedXmpLength;
			int extendedDigestLength;
			try
			{
				XMPUtils_PackageForJPEG(xmpCore.Handle, out pStandardXmp, out standardXmpLength, out pExtendedXmp, out extendedXmpLength, out pExtendedDigest, out extendedDigestLength);

				if (standardXmpLength > 0 && pStandardXmp != IntPtr.Zero)
				{
					standardXmp = MarshalHelper.GetString(pStandardXmp, 0, standardXmpLength, Encoding.UTF8);
					Common_FreeString(pStandardXmp);
				}
				else
				{
					standardXmp = null;
				}

				if (extendedXmpLength > 0 && pExtendedXmp != IntPtr.Zero)
				{
					extendedXmp = MarshalHelper.GetString(pExtendedXmp, 0, extendedXmpLength, Encoding.UTF8);
					Common_FreeString(pExtendedXmp);
				}
				else
				{
					extendedXmp = null;
				}

				if (extendedDigestLength > 0 && pExtendedDigest != IntPtr.Zero)
				{
					extendedDigest = MarshalHelper.GetString(pExtendedDigest, 0, extendedDigestLength, Encoding.UTF8);
					Common_FreeString(pExtendedDigest);
				}
				else
				{
					extendedDigest = null;
				}
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
		}

		public static void MergeFromJPEG(XmpCore fullXmp, XmpCore extendedXmp)
		{
			try
			{
				XMPUtils_MergeFromJPEG(fullXmp.Handle, extendedXmp.Handle);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_PackageForJPEG", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_PackageForJPEG(IntPtr xmpCore, out IntPtr standardXMP, out int standardXMPLength, out IntPtr extendedXMP, out int extendedXMPLength, out IntPtr extendedDigest, out int extendedDigestLength);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_MergeFromJPEG", CharSet = CharSet.Auto)]
		private static extern void XMPUtils_MergeFromJPEG(IntPtr fullXmp, IntPtr extendedXmp);

		#endregion

		#region UI helper functions

		public static void CatenateArrayItems(XmpCore xmpCore, string schemaNS, string arrayName, string separator, string quotes, ArrayItemFlags options, out string catedStr)
		{
			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pArrayName = IntPtr.Zero;
			IntPtr pSeparator = IntPtr.Zero;
			IntPtr pQuotes = IntPtr.Zero;

			IntPtr pCatedStr;
			int catedStrLength;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pArrayName = MarshalHelper.GetString(arrayName, Encoding.UTF8);
				pSeparator = MarshalHelper.GetString(separator, Encoding.UTF8);
				pQuotes = MarshalHelper.GetString(quotes, Encoding.UTF8);
				XMPUtils_CatenateArrayItems(xmpCore.Handle, pSchemaNS, pArrayName, pSeparator, pQuotes, options, out pCatedStr, out catedStrLength);

				if (catedStrLength > 0 && pCatedStr != IntPtr.Zero)
				{
					catedStr = MarshalHelper.GetString(pCatedStr, 0, catedStrLength, Encoding.UTF8);
					Common_FreeString(pCatedStr);
				}
				else
				{
					catedStr = null;
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
				MarshalHelper.FreeString(pSeparator);
				MarshalHelper.FreeString(pQuotes);
			}
		}

		public static void SeparateArrayItems(XmpCore xmpCore, string schemaNS, string arrayName, ArrayItemFlags options, string catedStr)
		{
			IntPtr pSchemaNS = IntPtr.Zero;
			IntPtr pArrayName = IntPtr.Zero;
			IntPtr pCatedString = IntPtr.Zero;
			try
			{
				pSchemaNS = MarshalHelper.GetString(schemaNS, Encoding.UTF8);
				pArrayName = MarshalHelper.GetString(arrayName, Encoding.UTF8);
				pCatedString = MarshalHelper.GetString(catedStr, Encoding.UTF8);
				XMPUtils_SeparateArrayItems(xmpCore.Handle, pSchemaNS, pArrayName, options, pCatedString);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSchemaNS);
				MarshalHelper.FreeString(pArrayName);
				MarshalHelper.FreeString(pCatedString);
			}
		}

		public static void RemoveProperties(XmpCore xmpCore, string schemaNS, string propName, RemoveMode options)
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
				XMPUtils_RemoveProperties(xmpCore.Handle, pSchemaNS, pPropName, options);
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

		public static void AppendProperties(XmpCore source, XmpCore dest, AppendMode options)
		{
			try
			{
				XMPUtils_AppendProperties(source.Handle, dest.Handle, options);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
		}

		public static void DuplicateSubtree(XmpCore source, XmpCore dest, string sourceNS, string sourceRoot, string destNS, string destRoot, PropertyFlags options)
		{
			IntPtr pSourceNS = IntPtr.Zero;
			IntPtr pSourceRoot = IntPtr.Zero;
			IntPtr pDestNS = IntPtr.Zero;
			IntPtr pDestRoot = IntPtr.Zero;
			try
			{
				pSourceNS = MarshalHelper.GetString(sourceNS, Encoding.UTF8);
				pSourceRoot = MarshalHelper.GetString(sourceRoot, Encoding.UTF8);
				pDestNS = MarshalHelper.GetString(destNS, Encoding.UTF8);
				pDestRoot = MarshalHelper.GetString(destRoot, Encoding.UTF8);
				XMPUtils_DuplicateSubtree(source.Handle, dest.Handle, pSourceNS, pSourceRoot, pDestNS, pDestRoot, options);
			}
			catch (Exception)
			{
				throw new XmpException("Exception occured in XmpToolkit.", (XmpErrorCode)Common_GetLastError());
			}
			finally
			{
				MarshalHelper.FreeString(pSourceNS);
				MarshalHelper.FreeString(pSourceRoot);
				MarshalHelper.FreeString(pDestNS);
				MarshalHelper.FreeString(pDestRoot);
			}
		}

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_CatenateArrayItems", CharSet = CharSet.Auto)]
		private static extern bool XMPUtils_CatenateArrayItems(IntPtr xmpCore, IntPtr schemaNS, IntPtr arrayName, IntPtr separator, IntPtr quotes, ArrayItemFlags options, out IntPtr catedStr, out int catedStrLength);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_SeparateArrayItems", CharSet = CharSet.Auto)]
		private static extern bool XMPUtils_SeparateArrayItems(IntPtr xmpCore, IntPtr schemaNS, IntPtr arrayName, ArrayItemFlags options, IntPtr catedStr);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_RemoveProperties", CharSet = CharSet.Auto)]
		private static extern bool XMPUtils_RemoveProperties(IntPtr xmpCore, IntPtr schemaNS, IntPtr propName, RemoveMode options);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_AppendProperties", CharSet = CharSet.Auto)]
		private static extern bool XMPUtils_AppendProperties(IntPtr source, IntPtr dest, AppendMode options);

		[DllImport("XmpToolkit", EntryPoint = "XMPUtils_DuplicateSubtree", CharSet = CharSet.Auto)]
		private static extern bool XMPUtils_DuplicateSubtree(IntPtr source, IntPtr dest, IntPtr sourceNS, IntPtr sourceRoot, IntPtr destNS, IntPtr destRoot, PropertyFlags options);

		#endregion

		[DllImport("XmpToolkit", EntryPoint = "Common_FreeString", CharSet = CharSet.Auto)]
		private static extern void Common_FreeString(IntPtr stringHandle);

		[DllImport("XmpToolkit", EntryPoint = "Common_GetLastError", CharSet = CharSet.Auto)]
		private static extern int Common_GetLastError();
	}
}
