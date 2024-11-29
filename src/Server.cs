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
        _ = Task.Run(() => HandleRequestAsync(clientSocket));

    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

async Task HandleRequestAsync(Socket socket)
{
    try
    {
        var buffer = new byte[1024];
        var bytesRead = await socket.ReceiveAsync(buffer, SocketFlags.None);

        var httpRequest = HttpRequest.ParseHttpRequest(buffer[..bytesRead]);
        
        if (httpRequest.Uri == "/")
        {
            await socket.SendAsync(new ArraySegment<byte>("HTTP/1.1 200 OK\r\n\r\n"u8.ToArray()), SocketFlags.None);
        }

        if (httpRequest.Uri.StartsWith("/echo/"))
        {
            var response = new HttpResponse
            {
                Body = httpRequest.Uri.Replace("/echo/", ""),
            };
            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response.ToString())), SocketFlags.None);
        }

        if (httpRequest.Uri == "/user-agent")
        {
            var response = new HttpResponse
            {
                Body = httpRequest.Headers["User-Agent"],
            };
            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response.ToString())), SocketFlags.None);
        }

        if (httpRequest.Method == "GET" && httpRequest.Uri.StartsWith("/files/"))
        {
            var fileName = httpRequest.Uri.Replace("/files/", "").Trim();
            var filePath = Path.Combine(args[1], fileName);
            if (!File.Exists(filePath))
            {
                await socket.SendAsync(new ArraySegment<byte>("HTTP/1.1 404 Not Found\r\n\r\n"u8.ToArray()), SocketFlags.None);
                return;
            }
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var response = new HttpResponse
            {
                Headers =
                {
                    ["Content-Length"] = fileBytes.Length.ToString(),
                    ["Content-Type"] = "application/octet-stream"
                },
                Body = Encoding.UTF8.GetString(fileBytes),
            };
            
            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response.ToString())), SocketFlags.None);
        }

        if (httpRequest.Method == "POST" && httpRequest.Uri.StartsWith("/files/"))
        {
            var fileName = httpRequest.Uri.Replace("/files/", "").Trim();
            var filePath = Path.Combine(args[1], fileName);
            if (string.IsNullOrWhiteSpace(httpRequest.Body))
            {
                await socket.SendAsync("HTTP/1.1 400 Bad Request\r\n\r\n"u8.ToArray());
                return;
            }
            var fileContents = Encoding.UTF8.GetBytes(httpRequest.Body);
            await File.WriteAllBytesAsync(filePath, fileContents);
            await socket.SendAsync("HTTP/1.1 201 Created\r\n\r\n"u8.ToArray());

        }
        else
        {
            await socket.SendAsync(new ArraySegment<byte>("HTTP/1.1 404 Not Found\r\n\r\n"u8.ToArray()), SocketFlags.None);
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





