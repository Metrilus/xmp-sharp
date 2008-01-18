using System;
using System.Collections.Generic;

namespace System.Text
{
	internal static class CsvEncoder
	{
		public static string Encode(List<string> strings)
		{
			StringBuilder result = new StringBuilder();
			char[] specialChars = new char[] { '"', ',', '\r', '\n' };
			foreach (string data in strings)
			{
				if (result.Length > 0)
				{
					result.Append(',');
				}
				if (data.IndexOfAny(specialChars) == -1)
				{
					result.Append(data.Trim());
				}
				else
				{
					result.Append('"' + data.Trim().Replace("\"", "\"\"") + '"');
				}
			}
			return result.ToString();
		}

		public static List<string> Decode(string csvString)
		{
			List<string> result = new List<string>();
			int pos = 0;
			char[] specialChars = new char[] { '"', ',' };
			while (pos < csvString.Length)
			{
				if (csvString[pos] == '"')
				{
					StringBuilder sb = new StringBuilder();
					pos++;
					while (true)
					{
						if (pos >= csvString.Length - 1)
						{
							if (sb.Length > 0)
							{
								result.Add(sb.ToString().Trim());
							}
							pos = csvString.Length;
							break;
						}
						if (csvString[pos] == '"')
						{
							if (csvString[pos + 1] == ',')
							{
								if (sb.Length > 0)
								{
									result.Add(sb.ToString().Trim());
									pos += 2;
									break;
								}
							}
							else
							{
								sb.Append('"');
								if (csvString[pos + 1] == '"')
								{
									// This is to be expected. If not, then the string is invalid CSV.
									pos++;
								}
								pos ++;
							}
						}
						else
						{
							sb.Append(csvString[pos]);
							pos++;
						}
					}
				}
				else
				{
					int commaPos = csvString.IndexOf(',', pos);
					if (commaPos == -1)
					{
						string s = csvString.Substring(pos).Trim();
						if (s.Length > 0)
						{
							result.Add(s);
						}
						pos = csvString.Length;
					}
					else
					{
						string s = csvString.Substring(pos, commaPos - pos).Trim();
						if (s.Length > 0)
						{
							result.Add(s);
						}
						pos = commaPos + 1;
					}
				}
			}
			return result;
		}
	}
}
