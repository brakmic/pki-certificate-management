using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace Common.Services;

public class CertificateProviderFreeIpa : ICertificateProvider
{
    private readonly string _certPath;
    private readonly string _keyPath;

    public CertificateProviderFreeIpa(string certPath, string keyPath)
    {
        _certPath = certPath;
        _keyPath = keyPath;
    }

    public X509Certificate2 GetServerCertificate()
    {
        var serverCertPath = Path.Combine("certs", "server.pem");
        var serverKeyPath = Path.Combine("certs", "server-key.pem");
        return X509Certificate2.CreateFromPemFile(serverCertPath, serverKeyPath);
    }

    public X509CertificateCollection GetClientCertificates()
    {
        var cert = X509Certificate2.CreateFromPemFile(_certPath, _keyPath);
        var collection = new X509CertificateCollection { cert };
        return collection;
    }
}
