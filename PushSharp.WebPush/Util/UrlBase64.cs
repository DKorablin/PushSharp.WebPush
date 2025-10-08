using System;

namespace WebPush.Util
{
	internal static class UrlBase64
	{
		/// <summary>Decodes a url-safe base64 string into bytes</summary>
		/// <param name="base64"></param>
		/// <returns></returns>
		public static Byte[] Decode(String base64)
		{
			base64 = base64.Replace('-', '+').Replace('_', '/');

			while(base64.Length % 4 != 0)
				base64 += "=";

			return Convert.FromBase64String(base64);
		}

		/// <summary>Encodes bytes into url-safe base64 string</summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static String Encode(Byte[] data)
			=> Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_').TrimEnd('=');
	}
}