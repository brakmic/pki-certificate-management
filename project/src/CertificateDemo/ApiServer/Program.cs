using Common.Services;
using Microsoft.AspNetCore.Server.Kestrel.Https;

var serverCertPath = "certs/server.pem";
var serverKeyPath = "certs/server-key.pem";
ICertificateProvider freeIpaProvider = new CertificateProviderFreeIpa(serverCertPath, serverKeyPath);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(opts =>
{
    opts.ListenAnyIP(8443, lopts => lopts.UseHttps(new HttpsConnectionAdapterOptions
    {
        ServerCertificate = freeIpaProvider.GetServerCertificate(),
        ClientCertificateMode = ClientCertificateMode.RequireCertificate,
        // Development:
        ClientCertificateValidation = (cert, chain, errors) => true
        // For production, use: chain!.Build(cert)
    }));
    // Force HTTP/1.1 (mTLS seems unreliable with Http/2.0)
    opts.ConfigureEndpointDefaults(e => e.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
});

builder.Services.AddControllers();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        Console.WriteLine($"[ERROR] Unhandled exception: {error}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { Error = error?.Message });
    });
});

app.MapControllers();
app.Run();
