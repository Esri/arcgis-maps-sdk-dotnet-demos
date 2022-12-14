Demo: .NET MAUI Sign In Sample
=======================
Required version: ArcGIS Maps SDK for .NET 200.0.0

Demonstrates using OAuth to sign into an ArcGIS Portal, retrieve and save an ArcGIS Maps SDK license, and store credentials persisting across application sessions.

### Notable classes:
* [`OAuthAuthorizeHandler.cs`](OAuthAuthorizeHandler.cs) - Integrates .NET MAUI's and WinUIEx' WebAuthentication Handler with ArcGIS Maps SDK for .NET.
* [`StartupPage.xaml.cs`](StartupPage.xaml.cs) - Configures the portal and OAuth, credential persistance in secure storage and ArcGIS Maps SDK license retrival.
