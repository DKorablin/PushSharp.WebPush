# PushSharp.WebPush

[![Auto build](https://github.com/DKorablin/PushSharp.WebPush/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/PushSharp.WebPush/releases/latest)
[![Nuget](https://img.shields.io/nuget/v/AlphaOmega.PushSharp.WebPush)](https://www.nuget.org/packages/AlphaOmega.PushSharp.WebPush)

## Overview

PushSharp.WebPush is a .NET library for sending Web Push notifications from backend servers, implementing the [Web Push Protocol](https://tools.ietf.org/html/draft-ietf-webpush-protocol) and [Message Encryption for Web Push](https://tools.ietf.org/html/draft-ietf-webpush-encryption) (VAPID, ECDH, AES-GCM). It supports legacy GCM for older browsers and is compatible with modern browsers and push services.

- **Supported frameworks:**
  - .NET Framework 4.8
  - .NET Standard 2.0 (usable from .NET Core 2.0+, .NET 5/6/7/8)
- **NuGet package:** [AlphaOmega.PushSharp.WebPush](https://www.nuget.org/packages/AlphaOmega.PushSharp.WebPush)
- **SourceLink enabled** for debugging into source

## What's new
1. Updated NuGet packages to the latest versions
2. Added assembly signature (PublicKeyToken=a8ac5fc45c3adb8d)
3. Added PE file signing. (S/N: 00c18bc05b61a77408c694bd3542d035)
4. Added CI/CD pipelines
5. Limited number of builds: .NET 4.8 and .NET Standard 2.0 only (I will gladly return the rest if needed.)
6. Removed generic options argument and replaced with strongly typed parameters.
7. Changed SetGcmApiKey(...) from method to property.
8. Marked GcmApiKey property as deprecated.

## Why

Web push requires that push messages triggered from a backend be done via the [Web Push Protocol](https://tools.ietf.org/html/draft-ietf-webpush-protocol) and if you want to send data with your push message, you must also encrypt that data according to the [Message Encryption for Web Push spec](https://tools.ietf.org/html/draft-ietf-webpush-encryption).

This package makes it easy to send messages and will also handle legacy support for browsers relying on GCM for message sending / delivery.

## Install

Install via NuGet:

```
Install-Package AlphaOmega.PushSharp.WebPush
```

- **Dependencies:** Portable.BouncyCastle, System.Text.Json
- **Supported on:** .NET Framework 4.8, .NET Standard 2.0, .NET 6/7/8

## Demo Project

- [ASP.NET MVC Core demo project (external)](https://github.com/coryjthompson/WebPushDemo)
- Local test project: see `WebPush.Test` in this repository (targets .NET 4.8 and .NET 8)

## Usage

The common use case for this library is an application server using VAPID keys (recommended) or a GCM API key (deprecated).

```csharp
using PushSharp.WebPush;

var pushEndpoint = "https://fcm.googleapis.com/fcm/send/efz_TLX_rLU:APA91bE6U0iybLYvv0F3mf6uDLB6....";
var p256dh = "BKK18ZjtENC4jdhAAg9OfJacySQiDVcXMamy3SKKy7FwJcI5E0DKO9v4V2Pb8NnAPN4EVdmhO............";
var auth = "fkJatBBEl...............";

var subject = "mailto:example@example.com";
var publicKey = "BDjASz8kkVBQJgWcD05uX3VxIs_gSHyuS023jnBoHBgUbg8zIJvTSQytR8MP4Z3-kzcGNVnM...............";
var privateKey = "mryM-krWj_6IsIMGsd8wNFXGBxnx...............";

var subscription = new PushSubscription(pushEndpoint, p256dh, auth);
var vapidDetails = new VapidDetails(subject, publicKey, privateKey);
// var gcmAPIKey = "[your key here]";

var webPushClient = new WebPushClient();
try
{
    await webPushClient.SendNotificationAsync(subscription, "payload", vapidDetails: vapidDetails);
    // await webPushClient.SendNotificationAsync(subscription, "payload", gcmAPIKey: gcmAPIKey);
}
catch (WebPushException exception)
{
    Console.WriteLine("Http STATUS code" + exception.StatusCode);
}
```

- **Note:** Use the correct namespace: `using PushSharp.WebPush;`
- **VAPID is recommended** for all modern browsers. GCM is deprecated and should only be used for legacy support.

## API Reference

### SendNotificationAsync

```csharp
Task SendNotificationAsync(PushSubscription subscription, string payload = null, VapidDetails vapidDetails = null, string gcmAPIKey = null, CancellationToken cancellationToken = default);
```

- `subscription`: a `PushSubscription` object containing endpoint, p256dh, and auth values.
- `payload`: optional string data to send (must be encrypted if provided).
- `vapidDetails`: a `VapidDetails` object with subject, publicKey, and privateKey (recommended).
- `gcmAPIKey`: (deprecated) GCM API key for legacy browsers.
- `cancellationToken`: optional.

> **Note:** You don't need to define a payload, and this method will work without a GCM API Key and/or VAPID keys if the push service supports it.

### GenerateVapidKeys

```csharp
VapidDetails vapidKeys = VapidHelper.GenerateVapidKeys();
Console.WriteLine($"Public {vapidKeys.PublicKey}");
Console.WriteLine($"Private {vapidKeys.PrivateKey}");
```

- Returns a `VapidDetails` object with URL Safe Base64 encoded public and private keys.
- **Tip:** Generate these once and store them securely for future use.

### GetVapidHeaders

```csharp
Uri uri = new Uri(subscription.Endpoint);
string audience = uri.Scheme + Uri.SchemeDelimiter + uri.Host;
Dictionary<string, string> vapidHeaders = VapidHelper.GetVapidHeaders(
  audience,
  "mailto:example@example.com",
  publicKey,
  privateKey
);
```

- Returns a dictionary with `Authorization` and `Crypto-Key` headers for use in custom HTTP requests.

### Deprecated: GcmApiKey

```csharp
webPushClient.GcmApiKey = "your-gcm-key";
```

- This property is deprecated and should only be used for legacy GCM support.

## Error Handling

- `WebPushException` is thrown for HTTP errors. Check `StatusCode` for details.
- Common status codes:
  - 201: Success
  - 404/410: Subscription expired or invalid (remove from your database)
  - 400: Bad request (check payload and keys)
  - 429: Rate limited

## Security Notes

- **Store VAPID private keys securely**; do not commit them to source control.
- Validate all subscription inputs.
- Do not log sensitive key material.

## Supported Browsers/Services

- Chrome, Firefox, Edge, and any browser supporting the Web Push Protocol.
- FCM (Firebase Cloud Messaging) for Chrome/Android.
- GCM (deprecated) for legacy support.

## Building Locally

- Prerequisites: .NET SDK 6.0+ or Visual Studio 2022+
- Build: `dotnet build` or open the solution in Visual Studio.
- Tests: see `WebPush.Test` project (net48, net8.0)

## Contributing

Contributions are welcome! Please open issues or pull requests. See `CONTRIBUTING.md` if available.

## License

This project is licensed under the [Mozilla Public License 2.0 (MPL-2.0)](LICENSE.md).

## Credits
- Ported from https://github.com/web-push-libs/web-push.
- Original Encryption code from https://github.com/LogicSoftware/WebPushEncryption
- Original WebPush authors: https://github.com/web-push-libs/web-push-csharp