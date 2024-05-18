using System.Collections.Generic;
using System.Text;

namespace Celeste
{
	public static class RunLengthEncoding
	{
		public static byte[] Encode(string str)
		{
			List<byte> bytes = new List<byte>();
			for (int i = 0; i < str.Length; i++)
			{
				byte num = 1;
				char chr;
				for (chr = str[i]; i + 1 < str.Length && str[i + 1] == chr; i++)
				{
					if (num >= byte.MaxValue)
					{
						break;
					}
					num = (byte)(num + 1);
				}
				bytes.Add(num);
				bytes.Add((byte)chr);
			}
			return bytes.ToArray();
		}

		public static string Decode(byte[] bytes)
		{
			StringBuilder str = new StringBuilder();
			for (int i = 0; i < bytes.Length; i += 2)
			{
				str.Append((char)bytes[i + 1], bytes[i]);
			}
			return str.ToString();
		}
	}
}
