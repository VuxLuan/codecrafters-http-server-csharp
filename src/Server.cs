using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_http_server;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
var server = new TcpListener(IPAddress.Any, 4221);
server.Start();


while (true)
{
    try
    {
        using Socket clientSocket = server.AcceptSocket();
        HandleRequest(clientSocket);
        clientSocket.Close();

    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

static void HandleRequest(Socket socket)
{
    var buffer = new byte[4096];
    var bytesRead = socket.Receive(buffer);

    var httpRequest = ParseHttpRequest(buffer[..bytesRead]);
    if (httpRequest.Uri == "/")
    {
        socket.Send("HTTP/1.1 200 OK\r\n\r\n"u8.ToArray());
    }
    else
    {
        socket.Send("HTTP/1.1 404 Not Found\r\n\r\n"u8.ToArray());
    }
}

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
static HttpRequest ParseHttpRequest(byte[] input)
{
   
    var data = Encoding.UTF8.GetString(input);
    var lines = data.Split(["\r\n"],StringSplitOptions.None);
    var requestLineParts = lines[0].Split(' ');
    var method = requestLineParts[0];
    var uri = requestLineParts[1];
    var protocol = requestLineParts[2];

    var headers = new Dictionary<string, string>();
    var i = 1;
    while (!string.IsNullOrWhiteSpace(lines[i]))
    {
        var headerParts = lines[i].Split([": "], 2, StringSplitOptions.None);
        headers[headerParts[0]] = headerParts[1];
        i++;
    }
    
    var body = string.Empty;
    if (!headers.TryGetValue("Content-Length", out var header))
        return new HttpRequest
        {
            Method = method,
            Uri = uri,
            Protocol = protocol,
            Headers = headers,
            Body = body
        };
    var contentLength = int.Parse(header);
    body = data[^contentLength..];

    return new HttpRequest
    {
        Method = method,
        Uri = uri,
        Protocol = protocol,
        Headers = headers,
        Body = body
    };
}



