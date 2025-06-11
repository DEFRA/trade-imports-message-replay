using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Microsoft.Identity.Client;

namespace Defra.TradeImportsMessageReplay.MessageReplay.BlobService;

/// <summary>
/// The Azure SDK doesn't use the CDP proxied Http Client by default, so previously we used the HTTPS_CLIENT env var to
/// send the requests via CDPs squid proxy. This code is intended to use the http client we already setup that uses the proxy
/// when the CDP_HTTPS_PROXY env var is set.
/// Code borrowed from https://anthonysimmon.com/overriding-msal-httpclient-with-ihttpclientfactory/
/// </summary>
/// <param name="httpClientFactory"></param>
[ExcludeFromCodeCoverage]
public class MsalHttpClientFactoryAdapter(IHttpClientFactory httpClientFactory) : IMsalHttpClientFactory
{
    public HttpClient GetHttpClient()
    {
        return httpClientFactory.CreateClient("Msal");
    }
}

[ExcludeFromCodeCoverage]
public class ConfidentialClientApplicationTokenCredential : TokenCredential
{
    private readonly string[] _scopes = ["https://storage.azure.com/.default"];

    private readonly IConfidentialClientApplication _app;

    public ConfidentialClientApplicationTokenCredential(IServiceProvider serviceProvider, BlobServiceOptions config)
    {
        var httpClientFactory = serviceProvider.GetRequiredService<MsalHttpClientFactoryAdapter>();

        _app = ConfidentialClientApplicationBuilder
            .Create(config.AzureClientId)
            .WithHttpClientFactory(httpClientFactory)
            .WithTenantId(config.AzureTenantId)
            .WithClientSecret(config.AzureClientSecret)
            .Build();
    }

    public override async ValueTask<AccessToken> GetTokenAsync(
        TokenRequestContext requestContext,
        CancellationToken cancellationToken
    )
    {
        var authResult = await _app.AcquireTokenForClient(_scopes).ExecuteAsync(cancellationToken);
        return new AccessToken(authResult.AccessToken, authResult.ExpiresOn);
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return GetTokenAsync(requestContext, cancellationToken).AsTask().GetAwaiter().GetResult();
    }
}
