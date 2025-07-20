using Common.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private void LogClientInfo(string action, MessageDto? msg = null)
    {
        var cert = HttpContext.Connection.ClientCertificate;
        Console.WriteLine($"[DEBUG] {action}");
        if (msg != null)
            Console.WriteLine($"[DEBUG] Sender: {msg.Sender}, Message: {msg.Message}");
        Console.WriteLine($"[DEBUG] Client Cert: {cert?.Subject}");
        Console.WriteLine($"[DEBUG] Cert Issuer: {cert?.Issuer}");
        Console.WriteLine($"[DEBUG] Cert Thumbprint: {cert?.Thumbprint}");
    }

    [HttpPost]
    public IActionResult PostMessage([FromBody] MessageDto msg)
    {
        LogClientInfo("Received POST /api/message", msg);
        var cert = HttpContext.Connection.ClientCertificate;
        var response = new MessageResponseDto(
            ClientSender: msg?.Sender ?? "",
            ClientMessage: msg?.Message ?? "",
            ServerMessage: $"Hello {msg?.Sender}, your message was received at {DateTime.UtcNow:O}!",
            ClientCertificate: cert?.Subject ?? "",
            ClientIssuer: cert?.Issuer ?? "",
            ClientThumbprint: cert?.Thumbprint ?? ""
        );
        return Ok(response);
    }

    [HttpGet("server-info")]
    public IActionResult GetServerInfo()
    {
        LogClientInfo("Received GET /api/message/server-info");
        return Ok(new
        {
            Server = "API Server",
            Time = DateTime.UtcNow,
            Message = "Welcome to the secure API server!"
        });
    }

    [HttpGet("client-info")]
    public IActionResult GetClientInfo()
    {
        LogClientInfo("Received GET /api/message/client-info");
        var cert = HttpContext.Connection.ClientCertificate;
        return Ok(new
        {
            Certificate = cert?.Subject ?? "",
            Issuer = cert?.Issuer ?? "",
            Thumbprint = cert?.Thumbprint ?? ""
        });
    }
}
