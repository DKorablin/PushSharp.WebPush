using System;

namespace WebPush
{
	/// <summary>
	/// Represents the VAPID (Voluntary Application Server Identification) details used to authenticate a web push request.
	/// Consists of a subject (contact), a public key and a private key.
	/// </summary>
	public class VapidDetails
	{
		/// <summary>Creates an empty instance of <see cref="VapidDetails"/>. Properties can be set individually.</summary>
		public VapidDetails()
		{
		}

		/// <summary>Creates a populated instance of <see cref="VapidDetails"/>.</summary>
		/// <param name="subject">This should be a URL or a 'mailto:' email address.</param>
		/// <param name="publicKey">The VAPID public key as a URL-safe Base64 encoded string.</param>
		/// <param name="privateKey">The VAPID private key as a URL-safe Base64 encoded string.</param>
		public VapidDetails(String subject, String publicKey, String privateKey)
		{
			this.Subject = subject;
			this.PublicKey = publicKey;
			this.PrivateKey = privateKey;
		}

		/// <summary>Contact information for the application server. Must be a valid absolute URL or a mailto: address.</summary>
		public String Subject { get; set; }

		/// <summary>The VAPID public key (uncompressed P-256) encoded as a URL-safe Base64 string.</summary>
		public String PublicKey { get; set; }

		/// <summary>The VAPID private key (P-256 scalar) encoded as a URL-safe Base64 string.</summary>
		public String PrivateKey { get; set; }

		/// <summary>Optional explicit expiration (Unix time seconds) for the VAPID JWT. Set to -1 to let the helper choose.</summary>
		public Int64 Expiration { get; set; } = -1;
	}
}