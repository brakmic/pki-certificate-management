using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography.X509Certificates;

public class IndexModel : PageModel
{
    public X509Certificate2? ClientCert { get; private set; }
    public void OnGet() => ClientCert = HttpContext.Connection.ClientCertificate;
}
