using System.Text;
using Microsoft.Extensions.Logging;

namespace TravelerCompanion;

public class LoggingHandler(ILogger <LoggingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestMessage = new StringBuilder();
        requestMessage.AppendLine($"{request.Method} {request.RequestUri?.AbsolutePath} HTTP/{request.Version}");
        requestMessage.AppendLine($"Host: {request.RequestUri?.Host}");
        foreach (var (key, value) in request.Headers)
        {
            requestMessage.AppendLine($"{key}: {string.Join(", ", value)}");
        }
        
        var response = await base.SendAsync(request, cancellationToken);
        
        var responseMessage = new StringBuilder();
        responseMessage.AppendLine($"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}");
        foreach (var (key, value) in response.Headers)
        {
            responseMessage.AppendLine($"{key}: {string.Join(", ", value)}");
        }

        responseMessage.AppendLine(await response.Content.ReadAsStringAsync(cancellationToken));
        
        if(response.IsSuccessStatusCode)
        {
            logger.LogTrace("{Request}", requestMessage.ToString());
            logger.LogTrace("{Response}", responseMessage.ToString());
        }
        else
        {
            logger.LogWarning("{Request}", requestMessage.ToString());
            logger.LogWarning("{Response}", responseMessage.ToString());
        }
        
        return response;
    }
}

