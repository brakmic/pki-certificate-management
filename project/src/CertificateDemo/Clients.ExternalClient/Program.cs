using Common.Services;
using Common.Models;
using System.Net.Http.Json;

var provider = new CertificateProviderCfssl();
var clientCert = provider.GetClientCertificates()[0];

var handler = new HttpClientHandler
{
  ClientCertificateOptions = ClientCertificateOption.Manual,
  SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
  // Production:
  // ServerCertificateCustomValidationCallback = (message, cert, chain, errors)
  //   => cert!.Subject.Contains("CN=api-server.acme.com")
  // Development:
  ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};
handler.ClientCertificates.Add(clientCert);

var client = new HttpClient(handler)
{
  BaseAddress = new Uri("https://api-server.acme.com:8443")
};


// Talk to the server
var response = await client.PostAsJsonAsync("api/message", new MessageDto("external-client", "Hello via CFSSL!"));
response.EnsureSuccessStatusCode();
var result = await response.Content.ReadFromJsonAsync<MessageResponseDto>();
Console.WriteLine($"[API Server] {result?.ServerMessage}");
Console.WriteLine($"[Client] {result?.ClientSender}: {result?.ClientMessage}");
Console.WriteLine($"[Cert] Subject: {result?.ClientCertificate}");
Console.WriteLine($"[Cert] Issuer: {result?.ClientIssuer}");
Console.WriteLine($"[Cert] Thumbprint: {result?.ClientThumbprint}");
