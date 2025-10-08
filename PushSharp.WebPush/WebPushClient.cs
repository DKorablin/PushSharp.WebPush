using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using WebPush.Model;
using WebPush.Util;

namespace WebPush
{
	/// <summary>Client for sending Web Push protocol notifications with optional payload encryption and support for VAPID and legacy GCM / FCM authorization.</summary>
	public class WebPushClient : IWebPushClient
	{
		// default TTL is 4 weeks.
		private const Int32 DefaultTtl = 2419200;
		private readonly HttpClientHandler _httpClientHandler;

		private String _gcmApiKey;
		private HttpClient _httpClient;
		private VapidDetails _vapidDetails;

		// Used so we only cleanup internally created http clients
		private Boolean _isHttpClientInternallyCreated;

		/// <summary>Creates a new instance of <see cref="WebPushClient"/> with an internally managed <see cref="HttpClient"/>.</summary>
		public WebPushClient()
		{
		}

		/// <summary>Creates a new instance using an externally provided <see cref="HttpClient"/>. Disposal is the caller's responsibility.</summary>
		/// <param name="httpClient">The <see cref="HttpClient"/> to use for sending requests.</param>
		public WebPushClient(HttpClient httpClient)
			=> this._httpClient = httpClient;

		/// <summary>Creates a new instance specifying an <see cref="HttpClientHandler"/> used when creating the internal <see cref="HttpClient"/>.</summary>
		/// <param name="httpClientHandler">The handler used when creating an internal <see cref="HttpClient"/>.</param>
		public WebPushClient(HttpClientHandler httpClientHandler)
			=> this._httpClientHandler = httpClientHandler;

		/// <summary>Provides the <see cref="HttpClient"/> used by the client, creating one if necessary.</summary>
		protected HttpClient HttpClient
		{
			get
			{
				if(this._httpClient != null)
					return this._httpClient;

				this._isHttpClientInternallyCreated = true;
				this._httpClient = _httpClientHandler == null
					? new HttpClient()
					: new HttpClient(_httpClientHandler);

				return this._httpClient;
			}
		}

		/// <inheritdoc />
		public void SetGcmApiKey(String gcmApiKey)
		{
			if(gcmApiKey == null)
			{
				this._gcmApiKey = null;
				return;
			}

			if(String.IsNullOrEmpty(gcmApiKey))
				throw new ArgumentNullException(nameof(gcmApiKey), "The GCM API Key should be a non-empty string or null.");

			this._gcmApiKey = gcmApiKey;
		}

		/// <inheritdoc />
		public void SetVapidDetails(VapidDetails vapidDetails)
		{
			VapidHelper.ValidateSubject(vapidDetails.Subject);
			VapidHelper.ValidatePublicKey(vapidDetails.PublicKey);
			VapidHelper.ValidatePrivateKey(vapidDetails.PrivateKey);

			this._vapidDetails = vapidDetails;
		}

		/// <inheritdoc />
		public void SetVapidDetails(String subject, String publicKey, String privateKey)
			=> this.SetVapidDetails(new VapidDetails(subject, publicKey, privateKey));

		/// <inheritdoc />
		public HttpRequestMessage GenerateRequestDetails(PushSubscription subscription, String payload, Dictionary<String, Object> options = null)
		{
			if(!Uri.IsWellFormedUriString(subscription.Endpoint, UriKind.Absolute))
				throw new ArgumentException("You must pass in a subscription with at least a valid endpoint", nameof(subscription));

			var request = new HttpRequestMessage(HttpMethod.Post, subscription.Endpoint);

			if(!String.IsNullOrEmpty(payload) && (String.IsNullOrEmpty(subscription.Auth) || String.IsNullOrEmpty(subscription.P256DH)))
				throw new ArgumentException("To send a message with a payload, the subscription must have 'auth' and 'p256dh' keys.", nameof(payload));

			var currentGcmApiKey = this._gcmApiKey;
			var currentVapidDetails = this._vapidDetails;
			var timeToLive = DefaultTtl;
			var extraHeaders = new Dictionary<String, Object>();

			if(options != null)
			{
				var validOptionsKeys = new List<String> { "headers", "gcmAPIKey", "vapidDetails", "TTL" };
				foreach(var key in options.Keys)
				{
					if(!validOptionsKeys.Contains(key))
						throw new ArgumentException(key + " is an invalid options. The valid options are" + String.Join(",", validOptionsKeys));
				}

				if(options.ContainsKey("headers"))
					extraHeaders = options["headers"] as Dictionary<String, Object>
						?? throw new ArgumentException("options.headers must be of type Dictionary<string,object>");

				if(options.ContainsKey("gcmAPIKey"))
					currentGcmApiKey = options["gcmAPIKey"] as String
						?? throw new ArgumentException("options.gcmAPIKey must be of type string");

				if(options.ContainsKey("vapidDetails"))
					currentVapidDetails = options["vapidDetails"] as VapidDetails
						?? throw new ArgumentException("options.vapidDetails must be of type VapidDetails");

				if(options.ContainsKey("TTL"))
					timeToLive = options["TTL"] as Int32? ?? throw new ArgumentException("options.TTL must be of type int");
			}

			String cryptoKeyHeader = null;
			request.Headers.Add("TTL", timeToLive.ToString());

			foreach(var header in extraHeaders)
				request.Headers.Add(header.Key, header.Value.ToString());

			if(!String.IsNullOrEmpty(payload))
			{
				if(String.IsNullOrEmpty(subscription.P256DH) || String.IsNullOrEmpty(subscription.Auth))
					throw new ArgumentException("Unable to send a message with payload to this subscription since it doesn't have the required encryption key", nameof(subscription));

				var encryptedPayload = EncryptPayload(subscription, payload);

				request.Content = new ByteArrayContent(encryptedPayload.Payload);
				request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
				request.Content.Headers.ContentLength = encryptedPayload.Payload.Length;
				request.Content.Headers.ContentEncoding.Add("aesgcm");
				request.Headers.Add("Encryption", "salt=" + encryptedPayload.Base64EncodeSalt());
				cryptoKeyHeader = "dh=" + encryptedPayload.Base64EncodePublicKey();
			} else
			{
				request.Content = new ByteArrayContent(new Byte[0]);
				request.Content.Headers.ContentLength = 0;
			}

			var isGcm = subscription.Endpoint.StartsWith("https://android.googleapis.com/gcm/send");
			var isFcm = subscription.Endpoint.StartsWith("https://fcm.googleapis.com/fcm/send/");

			if(isGcm)
			{
				if(!String.IsNullOrEmpty(currentGcmApiKey))
					request.Headers.TryAddWithoutValidation("Authorization", "key=" + currentGcmApiKey);
			} else if(currentVapidDetails != null)
			{
				var uri = new Uri(subscription.Endpoint);
				var audience = uri.Scheme + "://" + uri.Host;

				var vapidHeaders = VapidHelper.GetVapidHeaders(audience, currentVapidDetails.Subject,
					currentVapidDetails.PublicKey, currentVapidDetails.PrivateKey, currentVapidDetails.Expiration);
				request.Headers.Add("Authorization", vapidHeaders["Authorization"]);

				if(String.IsNullOrEmpty(cryptoKeyHeader))
					cryptoKeyHeader = vapidHeaders["Crypto-Key"];
				else
					cryptoKeyHeader += ";" + vapidHeaders["Crypto-Key"];
			} else if(isFcm && !String.IsNullOrEmpty(currentGcmApiKey))
				request.Headers.TryAddWithoutValidation("Authorization", "key=" + currentGcmApiKey);

			request.Headers.Add("Crypto-Key", cryptoKeyHeader);
			return request;
		}

		private static EncryptionResult EncryptPayload(PushSubscription subscription, String payload)
		{
			try
			{
				return Encryptor.Encrypt(subscription.P256DH, subscription.Auth, payload);
			} catch(Exception ex)
			{
				if(ex is FormatException || ex is ArgumentException)
					throw new InvalidEncryptionDetailsException("Unable to encrypt the payload with the encryption key of this subscription.", subscription);

				throw;
			}
		}

		/// <inheritdoc />
		public void SendNotification(PushSubscription subscription, String payload = null, Dictionary<String, Object> options = null)
			=> this.SendNotificationAsync(subscription, payload, options).ConfigureAwait(false).GetAwaiter().GetResult();

		/// <inheritdoc />
		public void SendNotification(PushSubscription subscription, String payload, VapidDetails vapidDetails)
		{
			var options = new Dictionary<String, Object> { ["vapidDetails"] = vapidDetails };
			this.SendNotification(subscription, payload, options);
		}

		/// <inheritdoc />
		public void SendNotification(PushSubscription subscription, String payload, String gcmApiKey)
		{
			var options = new Dictionary<String, Object> { ["gcmAPIKey"] = gcmApiKey };
			this.SendNotification(subscription, payload, options);
		}

		/// <inheritdoc />
		public async Task SendNotificationAsync(PushSubscription subscription, String payload = null, Dictionary<String, Object> options = null, CancellationToken cancellationToken = default)
		{
			var request = this.GenerateRequestDetails(subscription, payload, options);
			var response = await this.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

			await HandleResponse(response, subscription).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task SendNotificationAsync(PushSubscription subscription, String payload,
			VapidDetails vapidDetails, CancellationToken cancellationToken = default)
		{
			var options = new Dictionary<String, Object> { ["vapidDetails"] = vapidDetails };
			await this.SendNotificationAsync(subscription, payload, options, cancellationToken).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task SendNotificationAsync(PushSubscription subscription, String payload, String gcmApiKey, CancellationToken cancellationToken = default)
		{
			var options = new Dictionary<String, Object> { ["gcmAPIKey"] = gcmApiKey };
			await this.SendNotificationAsync(subscription, payload, options, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>Handle Web Push responses.</summary>
		/// <param name="response">The HTTP response returned by the push service.</param>
		/// <param name="subscription">The subscription used for the request.</param>
		private static async Task HandleResponse(HttpResponseMessage response, PushSubscription subscription)
		{
			// Successful
			if(response.IsSuccessStatusCode)
				return;

			// Error
			var responseCodeMessage = "Received unexpected response code: " + (Int32)response.StatusCode;
			switch(response.StatusCode)
			{
			case HttpStatusCode.BadRequest:
				responseCodeMessage = "Bad Request";
				break;

			case HttpStatusCode.RequestEntityTooLarge:
				responseCodeMessage = "Payload too large";
				break;

			case (HttpStatusCode)429:
				responseCodeMessage = "Too many request";
				break;

			case HttpStatusCode.NotFound:
			case HttpStatusCode.Gone:
				responseCodeMessage = "Subscription no longer valid";
				break;
			}

			String details = null;
			if(response.Content != null)
				details = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			var message = String.IsNullOrEmpty(details)
				? responseCodeMessage
				: $"{responseCodeMessage}. Details: {details}";

			throw new WebPushException(message, subscription, response);
		}

		/// <summary>Disposes the underlying <see cref="HttpClient"/> if it was created internally.</summary>
		public void Dispose()
		{
			if(this._httpClient != null && this._isHttpClientInternallyCreated)
			{
				this._httpClient.Dispose();
				this._httpClient = null;
			}
		}
	}
}