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

    var httpRequest = HttpRequest.ParseHttpRequest(buffer[..bytesRead]);
    if (httpRequest.Uri == "/")
    {
        socket.Send("HTTP/1.1 200 OK\r\n\r\n"u8.ToArray());
    }

    if (httpRequest.Uri.Contains("/echo/"))
    {

        var response = new HttpResponse
        {
            StatusCode = 200,
            ReasonPhrase = "OK",
            Body = httpRequest.Uri.Replace("/echo/", "")
        };
        response.Headers["Content-Type"] = "text/plain";
        response.Headers["Content-Length"] = response.Body?.Length.ToString() ?? "0";
        socket.Send(Encoding.UTF8.GetBytes(response.ToString()));
    }
    else
    {
        socket.Send("HTTP/1.1 404 Not Found\r\n\r\n"u8.ToArray());
    }
}





