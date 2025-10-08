using System;

namespace WebPush.Model
{
	/// <summary>Exception thrown when a payload cannot be encrypted with the provided subscription's encryption parameters.</summary>
	public class InvalidEncryptionDetailsException : Exception
	{
		/// <summary>Creates a new <see cref="InvalidEncryptionDetailsException"/> instance.</summary>
		/// <param name="message">Description of the encryption failure.</param>
		/// <param name="pushSubscription">The subscription whose encryption parameters were invalid.</param>
		public InvalidEncryptionDetailsException(String message, PushSubscription pushSubscription)
			: base(message)
			=> this.PushSubscription = pushSubscription;

		/// <summary>The subscription whose encryption parameters were invalid.</summary>
		public PushSubscription PushSubscription { get; }
	}
}