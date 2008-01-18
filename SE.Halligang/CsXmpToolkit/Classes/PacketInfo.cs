using System;

namespace SE.Halligang.CsXmpToolkit
{
	/// <summary>
	/// 
	/// </summary>
	public class PacketInfo
	{
		private long offset = 0;
		/// <summary>
		/// 
		/// </summary>
		public long Offset
		{
			get { return offset; }
			internal set { offset = value; }
		}

		private int length = 0;
		/// <summary>
		/// 
		/// </summary>
		public int Length
		{
			get { return length; }
			internal set { length = value; }
		}

		private int padSize = 0;
		/// <summary>
		/// 
		/// </summary>
		public int PadSize
		{
			get { return padSize; }
			internal set { padSize = value; }
		}

		private byte charForm = 0;
		/// <summary>
		/// 
		/// </summary>
		public byte CharForm
		{
			get { return charForm; }
			internal set { charForm = value; }
		}

		private bool writeable = false;
		/// <summary>
		/// 
		/// </summary>
		public bool Writeable
		{
			get { return writeable; }
			internal set { writeable = value; }
		}
	}
}
