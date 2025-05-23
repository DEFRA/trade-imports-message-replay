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

/// <summary>
/// Takes care of retriving a token via ConfidentialClientApplicationBuilder
/// which allows us to inject our CDP_HTTPS_PROXY based http client.
///
/// It's unclear why this isn't available out of the box!
/// - IMsalHttpClientFactory isn't used by ClientSecretCredential
/// - The ClientSecretCredential has an internal constructor accepting MsalConfidentialClient but nothing seems to use it
/// - MsalConfidentialClient is itself internal
/// </summary>
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

    public override ValueTask<AccessToken> GetTokenAsync(
        TokenRequestContext requestContext,
        CancellationToken cancellationToken
    )
    {
        var authResult = _app.AcquireTokenForClient(_scopes).ExecuteAsync(cancellationToken).Result;
        return ValueTask.FromResult(new AccessToken(authResult.AccessToken, authResult.ExpiresOn));
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var authResult = _app.AcquireTokenForClient(_scopes).ExecuteAsync(cancellationToken).Result;
        return new AccessToken(authResult.AccessToken, authResult.ExpiresOn);
    }
}
