using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WebPush
{
	/// <summary>
	/// Represents an error returned while attempting to send a Web Push notification, exposing
	/// the original <see cref="HttpResponseMessage"/>, status code, headers and the subscription.
	/// </summary>
	public class WebPushException : Exception
	{
		/// <summary>Creates a new <see cref="WebPushException"/> instance.</summary>
		/// <param name="message">A human readable message describing the error.</param>
		/// <param name="pushSubscription">The subscription related to the failed request.</param>
		/// <param name="responseMessage">The HTTP response returned by the push service.</param>
		public WebPushException(String message, PushSubscription pushSubscription, HttpResponseMessage responseMessage) : base(message)
		{
			this.PushSubscription = pushSubscription;
			this.HttpResponseMessage = responseMessage;
		}

		/// <summary>Gets the HTTP status code returned by the push service.</summary>
		public HttpStatusCode StatusCode => this.HttpResponseMessage.StatusCode;

		/// <summary>Gets the HTTP response headers returned by the push service.</summary>
		public HttpResponseHeaders Headers => this.HttpResponseMessage.Headers;

		/// <summary>Gets or sets the subscription associated with the failed notification attempt.</summary>
		public PushSubscription PushSubscription { get; set; }

		/// <summary>Gets or sets the <see cref="HttpResponseMessage"/> from the push service.</summary>
		public HttpResponseMessage HttpResponseMessage { get; set; }
	}
}