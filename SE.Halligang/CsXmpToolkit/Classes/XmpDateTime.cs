using System;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;

namespace SE.Halligang.CsXmpToolkit
{
	internal static class XmpDateTime
	{
		public static DateTime XmpStringToDateTime(string xmpDateTime)
		{
			PInvoke.XmpDateTime xmpdt;
			XmpUtils.ConvertToDate(xmpDateTime, out xmpdt);
			return XmpDateTimeToDateTime(xmpdt);
		}

		internal static DateTime XmpDateTimeToDateTime(PInvoke.XmpDateTime xmpDateTime)
		{
			return new DateTime(xmpDateTime.Year, xmpDateTime.Month, xmpDateTime.Day, xmpDateTime.Hour,
				xmpDateTime.Minute, xmpDateTime.Second, xmpDateTime.NanoSecond, DateTimeKind.Local);
		}

		public static string DateTimeToXmpString(DateTime dateTime, TimeZone localTimeZone)
		{
			LogFile log = LogFile.GetInstance("CsXmpToolkit");
			log.AppendString(TraceLevel.Verbose, MethodInfo.GetCurrentMethod(), "Converting: " + dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
			PInvoke.XmpDateTime xmpdt = DateTimeToXmpDateTime(dateTime, localTimeZone);
			string value;
			XmpUtils.ConvertFromDate(xmpdt, out value);
			log.AppendString(TraceLevel.Verbose, MethodInfo.GetCurrentMethod(), "Result: " + value);
			return value;
		}

		internal static PInvoke.XmpDateTime DateTimeToXmpDateTime(DateTime dateTime, TimeZone localTimeZone)
		{
			LogFile log = LogFile.GetInstance("CsXmpToolkit");
			TimeSpan offset = localTimeZone.GetUtcOffset(dateTime);
			PInvoke.TimeZoneSign sign = PInvoke.TimeZoneSign.IsUtc;
			if (offset.Hours < 0 || offset.Minutes < 0)
			{
				sign = PInvoke.TimeZoneSign.WestOfUtc;
			}
			else if (offset.Hours > 0 || offset.Minutes > 0)
			{
				sign = PInvoke.TimeZoneSign.EastOfUtc;
			}
			log.AppendString(TraceLevel.Verbose, MethodInfo.GetCurrentMethod(), "Sign: " + sign.ToString());
			log.AppendString(TraceLevel.Verbose, MethodInfo.GetCurrentMethod(), "Offset: " + offset.Hours.ToString() + " hours, " + offset.Minutes.ToString() + " minutes");

			PInvoke.XmpDateTime xmpDateTime = new PInvoke.XmpDateTime();
			xmpDateTime.Year = dateTime.Year;
			xmpDateTime.Month = dateTime.Month;
			xmpDateTime.Day = dateTime.Day;
			xmpDateTime.Hour = dateTime.Hour;
			xmpDateTime.Minute = dateTime.Minute;
			xmpDateTime.Second = dateTime.Second;
			xmpDateTime.NanoSecond = 0; // dateTime.Millisecond;
			xmpDateTime.TZSign = sign;
			xmpDateTime.TZHour = Math.Abs(offset.Hours);
			xmpDateTime.TZMinute = Math.Abs(offset.Minutes);

			return xmpDateTime;
		}
	}
}
