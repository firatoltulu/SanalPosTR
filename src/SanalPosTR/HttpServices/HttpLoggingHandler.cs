using Serilog;
using Serilog.Context;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SanalPosTR
{
    public class HttpLoggingHandler : DelegatingHandler
    {
        private static readonly string[] TextBasedContentTypes = { "html", "text", "xml", "json", "txt", "x-www-form-urlencoded" };

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var method = request.Method.ToString();
            var requestUri = request.RequestUri?.ToString();

            using (LogContext.PushProperty("HttpProcessId", Guid.NewGuid()))
            {
                var requestHeaders = request.Headers.ToDictionary(h => h.Key, h => h.Value);
                var requestContent = string.Empty;

                if (request.Content != null)
                {
                    foreach (var header in request.Content.Headers)
                    {
                        requestHeaders[header.Key] = header.Value;
                    }

                    if (IsTextBasedContent(request.Content.Headers))
                    {
                        requestContent = await request.Content.ReadAsStringAsync();
                    }
                }

                HttpResponseMessage response;
                try
                {
                    response = await base.SendAsync(request, cancellationToken);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    Log.Error(ex, "HTTP {Method} failed. Uri: {RequestUri}, Elapsed: {ElapsedMs}ms", method, requestUri, stopwatch.ElapsedMilliseconds);
                    throw;
                }

                var responseContent = string.Empty;

                if (response.Content != null && IsTextBasedContent(response.Content.Headers))
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                }

                stopwatch.Stop();

                var sb = new StringBuilder();
                sb.Append("Request completed in ")
                  .Append(requestUri).Append(' ')
                  .Append(stopwatch.ElapsedMilliseconds).AppendLine(" ms")
                  .AppendLine("═══════════════════════════════════════════════════════════════")
                  .AppendLine("═════════════════════════════REQUEST═══════════════════════════")
                  .AppendLine("Parameters:")
                  .AppendLine(JsonSerializer.Serialize(requestHeaders.Select(p => new { name = p.Key, value = string.Join(", ", p.Value) })))
                  .AppendLine(requestContent)
                  .AppendLine("═════════════════════════════RESPONSE══════════════════════════")
                  .Append("StatusCode: ").Append((int)response.StatusCode).Append(' ').AppendLine(response.ReasonPhrase)
                  .AppendLine("Parameters:")
                  .AppendLine(JsonSerializer.Serialize(response.Headers.Select(p => new { name = p.Key, value = string.Join(", ", p.Value) })))
                  .AppendLine(responseContent)
                  .AppendLine("═══════════════════════════════════════════════════════════════");

                Log.Information(sb.ToString());

                return response;
            }
        }

        private static bool IsTextBasedContent(System.Net.Http.Headers.HttpContentHeaders headers)
        {
            if (headers.ContentType == null)
                return false;

            var mediaType = headers.ContentType.MediaType?.ToLowerInvariant() ?? string.Empty;

            foreach (var type in TextBasedContentTypes)
            {
                if (mediaType.Contains(type))
                    return true;
            }

            return false;
        }
    }
}
