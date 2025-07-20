using Common.Services;
using Microsoft.AspNetCore.Server.Kestrel.Https;

var builder = WebApplication.CreateBuilder(args);
ICertificateProvider certProvider = new CertificateProviderCfssl();

builder.WebHost.ConfigureKestrel(opts =>
{
    opts.ListenAnyIP(9443, lopts => lopts.UseHttps(new HttpsConnectionAdapterOptions
    {
        ServerCertificate = certProvider.GetServerCertificate(),
        ClientCertificateMode = ClientCertificateMode.AllowCertificate,
        // Production:
        //ClientCertificateValidation = (cert, chain, errors) => chain!.Build(cert)
        // Development:
        ClientCertificateValidation = (cert, chain, errors) => true
    }));
});

builder.Services.AddControllers();
builder.Services.AddRazorPages();
var app = builder.Build();
app.UseStaticFiles();
app.MapRazorPages();
app.Run();
