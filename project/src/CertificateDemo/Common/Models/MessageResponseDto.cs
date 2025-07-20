namespace Common.Models;
public record MessageResponseDto(
    string ClientSender,
    string ClientMessage,
    string ServerMessage,
    string ClientCertificate,
    string ClientIssuer,
    string ClientThumbprint
);
