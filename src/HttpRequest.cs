using System.Text;

namespace codecrafters_http_server;

public class HttpRequest
{
    public  string Method { get; set; } = string.Empty;
    public  string Uri { get; set; } = string.Empty;
    public string Protocol { get; set; } = "HTTP/1.1";
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }

// GET /index.html HTTP/1.1\r\nHost: localhost:4221\r\nUser-Agent: curl/7.64.1\r\nAccept: */*\r\n\r\n
/*
  // Request line
   GET                          // HTTP method
   /index.html                  // Request target
   HTTP/1.1                     // HTTP version
   \r\n                         // CRLF that marks the end of the request line

   // Headers
   Host: localhost:4221\r\n     // Header that specifies the server's host and port
   User-Agent: curl/7.64.1\r\n  // Header that describes the client's user agent
   Accept: * /*\r\n              // Header that specifies which media types the client can accept
   \r\n                         // CRLF that marks the end of the headers

   // Request body (empty)
 */

    public static HttpRequest ParseHttpRequest(byte[] input)
    {
        var request = new HttpRequest();
        var data = Encoding.UTF8.GetString(input);
        var lines = data.Split(["\r\n"],StringSplitOptions.None);
        var requestLineParts = lines[0].Split(' ');

        if (requestLineParts.Length == 3)
        {
            request.Method = requestLineParts[0];
            request.Uri = requestLineParts[1];
            request.Protocol = requestLineParts[2];
        }
        
        var i = 1;
        while (i < lines.Length && lines[i] != string.Empty)
        {
            var header = lines[i].Split(':', 2);
            if (header.Length == 2)
            {
                string headerName = header[0].Trim();
                string headerValue = header[1].Trim();
                request.Headers[headerName] = headerValue;
            }
            i++;
        }

        if (!request.Headers.ContainsKey("Content-Length") ||
            !int.TryParse(request.Headers["Content-Length"], out int contentLength)) return request;
        string body = data.Split("\r\n\r\n", 2)[1];
        if (body.Length >= contentLength)
        {
            request.Body = body;
        }
        
        return request;
        
    }
}