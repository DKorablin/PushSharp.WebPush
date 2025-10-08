using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebPush.Util;

namespace WebPush.Test
{
	[TestClass]
	public class VapidHelperTest
	{
		private const String ValidAudience = "http://example.com";
		private const String ValidSubject = "http://example.com/example";
		private const String ValidSubjectMailto = "mailto:example@example.com";

		private const String TestPublicKey = "BCvKwB2lbVUYMFAaBUygooKheqcEU-GDrVRnu8k33yJCZkNBNqjZj0VdxQ2QIZa4kV5kpX9aAqyBKZHURm6eG1A";

		private const String TestPrivateKey = "on6X5KmLEFIVvPP3cNX9kE0OF6PV9TJQXVbnKU2xEHI";

		[TestMethod]
		public void TestGenerateVapidKeys()
		{
			var keys = VapidHelper.GenerateVapidKeys();
			var publicKey = UrlBase64.Decode(keys.PublicKey);
			var privateKey = UrlBase64.Decode(keys.PrivateKey);

			Assert.HasCount(32, privateKey);
			Assert.HasCount(65, publicKey);
		}

		[TestMethod]
		public void TestGenerateVapidKeysNoCache()
		{
			var keys1 = VapidHelper.GenerateVapidKeys();
			var keys2 = VapidHelper.GenerateVapidKeys();

			Assert.AreNotEqual(keys1.PublicKey, keys2.PublicKey);
			Assert.AreNotEqual(keys1.PrivateKey, keys2.PrivateKey);
		}

		[TestMethod]
		public void TestGetVapidHeaders()
		{
			var publicKey = TestPublicKey;
			var privateKey = TestPrivateKey;
			var headers = VapidHelper.GetVapidHeaders(ValidAudience, ValidSubject, publicKey, privateKey);

			Assert.IsTrue(headers.ContainsKey("Authorization"));
			Assert.IsTrue(headers.ContainsKey("Crypto-Key"));
		}

		[TestMethod]
		public void TestGetVapidHeadersAudienceNotAUrl()
		{
			var publicKey = TestPublicKey;
			var privateKey = TestPrivateKey;
			Assert.Throws<ArgumentException>(() => VapidHelper.GetVapidHeaders("invalid audience", ValidSubjectMailto, publicKey, privateKey));
		}

		[TestMethod]
		public void TestGetVapidHeadersInvalidPrivateKey()
		{
			var publicKey = UrlBase64.Encode(new Byte[65]);
			var privateKey = UrlBase64.Encode(new Byte[1]);

			Assert.Throws<ArgumentException>(() => VapidHelper.GetVapidHeaders(ValidAudience, ValidSubject, publicKey, privateKey));
		}

		[TestMethod]
		public void TestGetVapidHeadersInvalidPublicKey()
		{
			var publicKey = UrlBase64.Encode(new Byte[1]);
			var privateKey = UrlBase64.Encode(new Byte[32]);

			Assert.Throws<ArgumentException>(() => VapidHelper.GetVapidHeaders(ValidAudience, ValidSubject, publicKey, privateKey));
		}

		[TestMethod]
		public void TestGetVapidHeadersSubjectNotAUrlOrMailTo()
		{
			var publicKey = TestPublicKey;
			var privateKey = TestPrivateKey;

			Assert.Throws<ArgumentException>(() => VapidHelper.GetVapidHeaders(ValidAudience, "invalid subject", publicKey, privateKey));
		}

		[TestMethod]
		public void TestGetVapidHeadersWithMailToSubject()
		{
			var publicKey = TestPublicKey;
			var privateKey = TestPrivateKey;
			var headers = VapidHelper.GetVapidHeaders(ValidAudience, ValidSubjectMailto, publicKey, privateKey);

			Assert.IsTrue(headers.ContainsKey("Authorization"));
			Assert.IsTrue(headers.ContainsKey("Crypto-Key"));
		}

		[TestMethod]
		public void TestExpirationInPastExceptions()
		{
			var publicKey = TestPublicKey;
			var privateKey = TestPrivateKey;

			Assert.Throws<ArgumentException>(() => VapidHelper.GetVapidHeaders(ValidAudience, ValidSubjectMailto, publicKey, privateKey, 1552715607));
		}
	}
}