
using System.Net.Http.Json;
using Common.Models;
using Common.Services;

var username = "client-internal"; // Change as needed for each client
var certPath = $"certs/{username}.pem";
var keyPath = $"certs/{username}-key.pem";

var provider = new CertificateProviderFreeIpa(certPath, keyPath);
var clientCert = provider.GetClientCertificates()[0];

var handler = new HttpClientHandler
{
    ClientCertificateOptions = ClientCertificateOption.Manual,
    SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};
handler.ClientCertificates.Add(clientCert);

using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api-server.internal.acme.com:8443") };
client.DefaultRequestVersion = new Version(1, 1);
client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;

try
{
  var response = await client.PostAsJsonAsync("api/message", new MessageDto("internal-client", "Hello from FreeIPA!"));
  response.EnsureSuccessStatusCode();
  var result = await response.Content.ReadFromJsonAsync<MessageResponseDto>();

  Console.WriteLine($"[API Server] {result?.ServerMessage}");
  Console.WriteLine($"[Client] {result?.ClientSender}: {result?.ClientMessage}");
  Console.WriteLine($"[Cert] Subject: {result?.ClientCertificate}");
  Console.WriteLine($"[Cert] Issuer: {result?.ClientIssuer}");
  Console.WriteLine($"[Cert] Thumbprint: {result?.ClientThumbprint}");
}
catch (Exception ex)
{
  Console.WriteLine($"[ERROR] {ex}");
}
