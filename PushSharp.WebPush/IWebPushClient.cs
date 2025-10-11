using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebPush
{
	/// <summary>Defines operations for sending Web Push notifications with optional payload encryption plus VAPID and legacy GCM/FCM support.</summary>
	public interface IWebPushClient : IDisposable
	{
		/// <summary>[deprecated] Gets or sets the Google Cloud Messaging API key to send with the GCM request.</summary>
		/// <remarks>When sending messages to a GCM endpoint you need to set the GCM API key by either calling setGcmApiKey() or passing in the API key as an option to sendNotification()</remarks>
		String GcmApiKey { get; set; }

		/// <summary>When marking requests where you want to define VAPID details, call this method before sendNotifications() or pass in the details and options to sendNotification.</summary>
		/// <param name="vapidDetails">The VAPID details (subject, public and private keys).</param>
		void SetVapidDetails(VapidDetails vapidDetails);

		/// <summary>When marking requests where you want to define VAPID details, call this method before sendNotifications() or pass in the details and options to sendNotification.</summary>
		/// <param name="subject">This must be either a URL or a 'mailto:' address</param>
		/// <param name="publicKey">The public VAPID key as a base64 encoded string</param>
		/// <param name="privateKey">The private VAPID key as a base64 encoded string</param>
		void SetVapidDetails(String subject, String publicKey, String privateKey);

		/// <summary>Gets an HttpRequestMessage for a push notification without sending it (throws ArgumentException if input invalid).</summary>
		/// <param name="subscription">The PushSubscription you wish to send the notification to.</param>
		/// <param name="payload">The payload you wish to send to the user.</param>
		/// <param name="gcmAPIKey">The GCM API key</param>
		/// <param name="vapidDetails">The vapid details for the notification.</param>
		/// <param name="ttl">Time to live for notification.</param>
		/// <param name="extraHeaders">The list of extra headers value that should be added to the request headers.</param>
		/// <returns>A HttpRequestMessage object that can be sent.</returns>
		HttpRequestMessage GenerateRequestDetails(PushSubscription subscription, String payload, String gcmAPIKey = null, VapidDetails vapidDetails = null, Int32? ttl = null, Dictionary<String, Object> extraHeaders = null);

		/// <summary>Sends a push notification with optional payload and options (throws on failure).</summary>
		/// <param name="subscription">The PushSubscription you wish to send the notification to.</param>
		/// <param name="payload">The payload you wish to send to the user</param>
		/// <param name="gcmAPIKey">The GCM API key</param>
		/// <param name="vapidDetails">The vapid details for the notification.</param>
		/// <param name="ttl">Time to live for notification.</param>
		/// <param name="extraHeaders">The list of extra headers value that should be added to the request headers.</param>
		void SendNotification(PushSubscription subscription, String payload = null, String gcmAPIKey = null, VapidDetails vapidDetails = null, Int32? ttl = null, Dictionary<String, Object> extraHeaders = null);

		/// <summary>Sends a push notification asynchronously with optional payload and options (throws on failure).</summary>
		/// <param name="subscription">The PushSubscription you wish to send the notification to.</param>
		/// <param name="payload">The payload you wish to send to the user</param>
		/// <param name="gcmAPIKey">The GCM API key</param>
		/// <param name="vapidDetails">The vapid details for the notification.</param>
		/// <param name="ttl">Time to live for notification.</param>
		/// <param name="extraHeaders">The list of extra headers value that should be added to the request headers.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		Task SendNotificationAsync(PushSubscription subscription, String payload = null, String gcmAPIKey = null, VapidDetails vapidDetails = null, Int32? ttl = null, Dictionary<String, Object> extraHeaders = null, CancellationToken cancellationToken = default);
	}
}