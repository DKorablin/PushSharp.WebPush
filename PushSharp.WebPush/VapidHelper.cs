using System;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Parameters;
using WebPush.Util;

namespace WebPush
{
	/// <summary>
	/// Helper utilities for generating and validating VAPID (Voluntary Application Server Identification)
	/// keys and headers used to authenticate Web Push requests.
	/// </summary>
	public static class VapidHelper
	{
		/// <summary>Generate vapid keys</summary>
		/// <returns>A <see cref="VapidDetails"/> instance containing a newly generated public/private key pair.</returns>
		public static VapidDetails GenerateVapidKeys()
		{
			var results = new VapidDetails();

			var keys = ECKeyHelper.GenerateKeys();
			var publicKey = ((ECPublicKeyParameters)keys.Public).Q.GetEncoded(false);
			var privateKey = ((ECPrivateKeyParameters)keys.Private).D.ToByteArrayUnsigned();

			results.PublicKey = UrlBase64.Encode(publicKey);
			results.PrivateKey = UrlBase64.Encode(ByteArrayPadLeft(privateKey, 32));

			return results;
		}

		/// <summary>This method takes the required VAPID parameters and returns the required header to be added to a Web Push Protocol Request.</summary>
		/// <param name="audience">This must be the origin of the push service.</param>
		/// <param name="subject">This should be a URL or a 'mailto:' email address</param>
		/// <param name="publicKey">The VAPID public key as a base64 encoded string</param>
		/// <param name="privateKey">The VAPID private key as a base64 encoded string</param>
		/// <param name="expiration">The expiration of the VAPID JWT.</param>
		/// <returns>A dictionary of header key/value pairs.</returns>
		public static Dictionary<String, String> GetVapidHeaders(String audience, String subject, String publicKey, String privateKey, Int64 expiration = -1)
		{
			ValidateAudience(audience);
			ValidateSubject(subject);
			ValidatePublicKey(publicKey);
			ValidatePrivateKey(privateKey);

			var decodedPrivateKey = UrlBase64.Decode(privateKey);

			if(expiration == -1)
				expiration = UnixTimeNow() + 43200;
			else
				ValidateExpiration(expiration);

			var header = new Dictionary<String, Object> { { "typ", "JWT" }, { "alg", "ES256" } };

			var jwtPayload = new Dictionary<String, Object> { { "aud", audience }, { "exp", expiration }, { "sub", subject } };

			var signingKey = ECKeyHelper.GetPrivateKey(decodedPrivateKey);

			var signer = new JwsSigner(signingKey);
			var token = signer.GenerateSignature(header, jwtPayload);

			var results = new Dictionary<String, String>
			{
				{"Authorization", "WebPush " + token}, {"Crypto-Key", "p256ecdsa=" + publicKey}
			};

			return results;
		}

		/// <summary>Validates the audience (origin) of the push service.</summary>
		/// <param name="audience">The audience value to validate.</param>
		/// <exception cref="ArgumentNullException">Thrown when audience is missing.</exception>
		/// <exception cref="ArgumentException">Thrown when audience is invalid.</exception>
		public static void ValidateAudience(String audience)
		{
			if(String.IsNullOrEmpty(audience))
				throw new ArgumentNullException(nameof(audience), "No audience could be generated for VAPID.");

			if(audience.Length == 0)
				throw new ArgumentException("The audience value must be a string containing the origin of a push service. " + audience,nameof(audience));

			if(!Uri.IsWellFormedUriString(audience, UriKind.Absolute))
				throw new ArgumentException("VAPID audience is not a url.", nameof(audience));
		}

		/// <summary>Validates the contact subject (URL or mailto) used in the VAPID claims.</summary>
		/// <param name="subject">The subject to validate.</param>
		/// <exception cref="ArgumentNullException">Thrown when the subject is missing.</exception>
		/// <exception cref="ArgumentException">Thrown when the subject is invalid.</exception>
		public static void ValidateSubject(String subject)
		{
			if(String.IsNullOrEmpty(subject))
				throw new ArgumentNullException(nameof(subject), "A subject is required");

			if(subject.Length == 0)
				throw new ArgumentException("The subject value must be a string containing a url or mailto: address.", nameof(subject));

			if(!subject.StartsWith("mailto:") && !Uri.IsWellFormedUriString(subject, UriKind.Absolute))
				throw new ArgumentException("Subject is not a valid URL or mailto: address", nameof(subject));
		}

		/// <summary>Validates a URL-safe Base64 encoded uncompressed P-256 public key (65 bytes decoded).</summary>
		/// <param name="publicKey">The public key to validate.</param>
		/// <exception cref="ArgumentNullException">Thrown when the key is missing.</exception>
		/// <exception cref="ArgumentException">Thrown when the key is incorrect length.</exception>
		public static void ValidatePublicKey(String publicKey)
		{
			if(String.IsNullOrEmpty(publicKey))
				throw new ArgumentNullException(nameof(publicKey), "Valid public key not set");

			var decodedPublicKey = UrlBase64.Decode(publicKey);

			if(decodedPublicKey.Length != 65)
				throw new ArgumentException("Vapid public key must be 65 characters long when decoded", nameof(publicKey));
		}

		/// <summary>Validates a URL-safe Base64 encoded P-256 private key (32 bytes decoded).</summary>
		/// <param name="privateKey">The private key to validate.</param>
		/// <exception cref="ArgumentNullException">Thrown when the key is missing.</exception>
		/// <exception cref="ArgumentException">Thrown when the key is invalid.</exception>
		public static void ValidatePrivateKey(String privateKey)
		{
			if(String.IsNullOrEmpty(privateKey))
				throw new ArgumentNullException(nameof(privateKey), "Valid private key not set");

			var decodedPrivateKey = UrlBase64.Decode(privateKey);

			if(decodedPrivateKey.Length != 32)
				throw new ArgumentException("Vapid private key should be 32 bytes long when decoded.", nameof(privateKey));
		}

		private static void ValidateExpiration(Int64 expiration)
		{
			if(expiration <= UnixTimeNow())
				throw new ArgumentException("Vapid expiration must be a Unix timestamp in the future", nameof(expiration));
		}

		private static Int64 UnixTimeNow()
		{
			var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
			return (Int64)timeSpan.TotalSeconds;
		}

		private static Byte[] ByteArrayPadLeft(Byte[] src, Int32 size)
		{
			var dst = new Byte[size];
			var startAt = dst.Length - src.Length;
			Array.Copy(src, 0, dst, startAt, src.Length);
			return dst;
		}
	}
}