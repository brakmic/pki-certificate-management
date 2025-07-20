using System.Security.Cryptography.X509Certificates;

namespace Common.Services;

public class CertificateProviderCfssl : ICertificateProvider
{
  private const string CertFolder = "certs";

  public X509Certificate2 GetServerCertificate()
  {
    var certPath = Path.Combine(CertFolder, "server.pem");
    var keyPath = Path.Combine(CertFolder, "server-key.pem");
    return X509Certificate2.CreateFromPemFile(certPath, keyPath);
  }

  public X509CertificateCollection GetClientCertificates()
  {
      var certPath = Path.Combine(CertFolder, "client-external.pem");
      var keyPath = Path.Combine(CertFolder, "client-external-key.pem");
  
      if (!File.Exists(certPath))
          throw new FileNotFoundException($"Certificate file not found: {certPath}");
      if (!File.Exists(keyPath))
          throw new FileNotFoundException($"Key file not found: {keyPath}");
  
      var cert = X509Certificate2.CreateFromPemFile(certPath, keyPath);
      var collection = new X509CertificateCollection { cert };
      return collection;
  }
}