using System.Security.Cryptography.X509Certificates;
namespace Common.Services;

public interface ICertificateProvider
{
  X509Certificate2 GetServerCertificate();
  X509CertificateCollection GetClientCertificates();
}
