using System;
using WebPush.Util;

namespace WebPush
{
	// @LogicSoftware
	// Originally From: https://github.com/LogicSoftware/WebPushEncryption/blob/master/src/EncryptionResult.cs
	/// <summary>Result of encrypting a push payload containing the generated salt, server public key and encrypted payload bytes.</summary>
	public class EncryptionResult
	{
		/// <summary>Gets or sets the server's ephemeral public key (uncompressed P-256).</summary>
		public Byte[] PublicKey { get; set; }

		/// <summary>Gets or sets the encrypted payload bytes including authentication tag.</summary>
		public Byte[] Payload { get; set; }

		/// <summary>Gets or sets the random salt used during HKDF derivation.</summary>
		public Byte[] Salt { get; set; }

		/// <summary>Returns the public key encoded as a URL-safe Base64 string.</summary>
		public String Base64EncodePublicKey()
			=> UrlBase64.Encode(this.PublicKey);

		/// <summary>Returns the salt encoded as a URL-safe Base64 string.</summary>
		public String Base64EncodeSalt()
			=> UrlBase64.Encode(this.Salt);
	}
}