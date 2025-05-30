using Microsoft.Extensions.Options;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Utils.Logging;

public class TraceContextDelegatingHandler(
    IOptions<TraceHeader> traceHeader,
    ITraceContextAccessor traceContextAccessor
) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        // Setting the trace context will take either the trace ID from the incoming
        // message headers or it will start a new trace ID that may be propagated onwards
        // to any nested HTTP calls or further message publishing
        traceContextAccessor.Context ??= new TraceContext { TraceId = Guid.NewGuid().ToString("N") };

        request.Headers.Add(traceHeader.Value.Name, traceContextAccessor.Context.TraceId);

        return base.SendAsync(request, cancellationToken);
    }
}
