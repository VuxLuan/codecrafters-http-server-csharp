using System.Net.Mime;
using System.Text;

namespace codecrafters_http_server;

public class HttpResponse
{
    public string Protocol { get; set; } = "HTTP/1.1"; // Default to HTTP/1.1
    public int StatusCode { get; set; } = 200;
    public string ReasonPhrase { get; set; } = "OK";
    public Dictionary<string, string> Headers { get; set; } = new()
    {
        { "Content-Type", "text/plain" }
    };
    public string? Body { get; set; }

    public HttpResponse() { }

    public HttpResponse(int statusCode, string reasonPhrase, string? body = null)
    {
        StatusCode = statusCode;
        ReasonPhrase = reasonPhrase;
        Body = body;
    }
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Body))
        {
            Headers["Content-Length"] = Encoding.UTF8.GetByteCount(Body).ToString();
        }
        else
        {
            Headers["Content-Length"] = "0";
        }
        var responseBuilder = new StringBuilder();
        responseBuilder.Append($"{Protocol} {StatusCode} {ReasonPhrase}\r\n");

        foreach (var header in Headers)
        {
            responseBuilder.Append($"{header.Key}: {header.Value}\r\n");
        }
        
        responseBuilder.Append("\r\n");

        if (!string.IsNullOrEmpty(Body))
        {
            responseBuilder.Append(Body);
        }
        
        return responseBuilder.ToString();
    }
}