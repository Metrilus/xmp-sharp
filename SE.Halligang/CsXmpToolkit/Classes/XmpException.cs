using System;

namespace SE.Halligang.CsXmpToolkit
{
	public class XmpException : Exception
	{
		internal XmpException(string message, XmpErrorCode errorCode)
			: base(message)
		{
			this.errorCode = errorCode;
		}

		internal XmpException(string message, XmpErrorCode errorCode, Exception innerException)
			: base(message, innerException)
		{
			this.errorCode = errorCode;
		}

		private XmpErrorCode errorCode;
		/// <summary>
		/// 
		/// </summary>
		public XmpErrorCode ErrorCode
		{
			get { return errorCode; }
			set { errorCode = value; }
		}	
	}
}
