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
		/// <summary>When sending messages to a GCM endpoint you need to set the GCM API key by either calling setGcmApiKey() or passing in the API key as an option to sendNotification()</summary>
		/// <param name="gcmApiKey">The API key to send with the GCM request.</param>
		void SetGcmApiKey(String gcmApiKey);

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
		/// <param name="payload">The payload you wish to send to the user</param>
		/// <param name="options">Options for the GCM API key and vapid keys can be passed in if they are unique for each notification.</param>
		/// <returns>A HttpRequestMessage object that can be sent.</returns>
		HttpRequestMessage GenerateRequestDetails(PushSubscription subscription, String payload, Dictionary<String, Object> options = null);

		/// <summary>Sends a push notification with optional payload and options (throws on failure).</summary>
		/// <param name="subscription">The PushSubscription you wish to send the notification to.</param>
		/// <param name="payload">The payload you wish to send to the user</param>
		/// <param name="options">Options for the GCM API key and vapid keys can be passed in if they are unique for each notification.</param>
		void SendNotification(PushSubscription subscription, String payload = null, Dictionary<String, Object> options = null);

		/// <summary>Sends a push notification using the provided VAPID details (throws on failure).</summary>
		/// <param name="subscription">The PushSubscription you wish to send the notification to.</param>
		/// <param name="payload">The payload you wish to send to the user</param>
		/// <param name="vapidDetails">The vapid details for the notification.</param>
		void SendNotification(PushSubscription subscription, String payload, VapidDetails vapidDetails);

		/// <summary>Sends a push notification using the provided GCM API key (throws on failure).</summary>
		/// <param name="subscription">The PushSubscription you wish to send the notification to.</param>
		/// <param name="payload">The payload you wish to send to the user</param>
		/// <param name="gcmApiKey">The GCM API key</param>
		void SendNotification(PushSubscription subscription, String payload, String gcmApiKey);

		/// <summary>Sends a push notification asynchronously with optional payload and options (throws on failure).</summary>
		/// <param name="subscription">The PushSubscription you wish to send the notification to.</param>
		/// <param name="payload">The payload you wish to send to the user</param>
		/// <param name="options">Options for the GCM API key and vapid keys can be passed in if they are unique for each notification.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		Task SendNotificationAsync(PushSubscription subscription, String payload = null, Dictionary<String, Object> options = null, CancellationToken cancellationToken = default);

		/// <summary>Sends a push notification asynchronously using the provided VAPID details (throws on failure).</summary>
		/// <param name="subscription">The PushSubscription you wish to send the notification to.</param>
		/// <param name="payload">The payload you wish to send to the user</param>
		/// <param name="vapidDetails">The vapid details for the notification.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		Task SendNotificationAsync(PushSubscription subscription, String payload, VapidDetails vapidDetails, CancellationToken cancellationToken = default);

		/// <summary>Sends a push notification asynchronously using the provided GCM API key (throws on failure).</summary>
		/// <param name="subscription">The PushSubscription you wish to send the notification to.</param>
		/// <param name="payload">The payload you wish to send to the user</param>
		/// <param name="gcmApiKey">The GCM API key</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		Task SendNotificationAsync(PushSubscription subscription, String payload, String gcmApiKey, CancellationToken cancellationToken = default);
	}
}