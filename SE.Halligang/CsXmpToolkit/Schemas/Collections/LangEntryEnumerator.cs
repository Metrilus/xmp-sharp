using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit.Schemas
{
	public class LangEntryEnumerator : IEnumerator<LangEntry>
	{
		internal LangEntryEnumerator(Dictionary<string, string> dictionary)
		{
			this.dictionary = dictionary;
			this.enumerator = dictionary.GetEnumerator();
		}

		private Dictionary<string, string>.Enumerator enumerator;
		private Dictionary<string, string> dictionary;
		private LangEntry current = null;

		#region IEnumerator<LangEntry> Members

		public LangEntry Current
		{
			get { return current; }
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
		}

		#endregion

		#region IEnumerator Members

		object System.Collections.IEnumerator.Current
		{
			get { return current; }
		}

		public bool MoveNext()
		{
			if (enumerator.MoveNext())
			{
				current = new LangEntry(enumerator.Current.Key, enumerator.Current.Value);
				return true;
			}
			else
			{
				current = null;
				return false;
			}
		}

		public void Reset()
		{
			current = null;
			enumerator = dictionary.GetEnumerator();
		}

		#endregion
	}
}
