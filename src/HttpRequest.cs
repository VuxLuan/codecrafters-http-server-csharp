namespace codecrafters_http_server;

public class HttpRequest
{
    public required string Method { get; set; }
    public required string Uri { get; set; }
    public required string Protocol { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }
}