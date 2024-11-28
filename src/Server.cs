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
        Socket clientSocket = await server.AcceptSocketAsync();
        _ = Task.Run(() => HandleRequest(clientSocket));

    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

void HandleRequest(Socket socket)
{
    try
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
                Body = httpRequest.Uri.Replace("/echo/", ""),
            };
            socket.Send(Encoding.UTF8.GetBytes(response.ToString()));
        }

        if (httpRequest.Uri == "/user-agent")
        {
            var response = new HttpResponse
            {
                Body = httpRequest.Headers["User-Agent"],
            };
            socket.Send(Encoding.UTF8.GetBytes(response.ToString()));
        }

        if (httpRequest.Uri.Contains("/files/"))
        {
            var fileName = httpRequest.Uri.Replace("/files/", "").Trim();
            var filePath = Path.Combine(args[1], fileName);
            if (!File.Exists(filePath))
            {
                socket.Send("HTTP/1.1 404 Not Found\r\n\r\n"u8.ToArray());
            }
            var fileBytes = File.ReadAllBytes(filePath);
            var response = new HttpResponse
            {
                Headers =
                {
                    ["Content-Length"] = fileBytes.Length.ToString(),
                    ["Content-Type"] = "application/octet-stream"
                },
                Body = Encoding.UTF8.GetString(fileBytes),
            };
            
            socket.Send(Encoding.UTF8.GetBytes(response.ToString()));
            
            
        }
        else
        {
            socket.Send("HTTP/1.1 404 Not Found\r\n\r\n"u8.ToArray());
        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error handling request: {e.Message}");
    }
    finally
    {
        socket.Close();
    }
}





