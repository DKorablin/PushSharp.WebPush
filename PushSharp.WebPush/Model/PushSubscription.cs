using System;

namespace WebPush
{
	/// <summary>Represents the endpoint and cryptographic keys required to send a push notification to a specific user agent (browser push subscription).</summary>
	public class PushSubscription
	{
		/// <summary>Creates an empty <see cref="PushSubscription"/>. Properties can be assigned individually.</summary>
		public PushSubscription()
		{
		}

		/// <summary>Creates a populated <see cref="PushSubscription"/>.</summary>
		/// <param name="endpoint">The push service endpoint URL.</param>
		/// <param name="p256dh">The client's public P-256 ECDH key (Base64 URL encoded).</param>
		/// <param name="auth">The client's auth secret (Base64 URL encoded).</param>
		public PushSubscription(String endpoint, String p256dh, String auth)
		{
			this.Endpoint = endpoint;
			this.P256DH = p256dh;
			this.Auth = auth;
		}

		/// <summary>The push service endpoint URL used for delivering the message.</summary>
		public String Endpoint { get; set; }

		/// <summary>The client's public P-256 ECDH key (Base64 URL encoded) used for encrypting payloads.</summary>
		public String P256DH { get; set; }

		/// <summary>The shared authentication secret (Base64 URL encoded) for encryption context</summary>
		public String Auth { get; set; }
	}
}