using System.Security.Cryptography.X509Certificates;


namespace Microsoft.Examples
{
    public class CertificateConfiguration
    {
        public static X509Certificate2 GetCertificate()
        {
            var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            try
            {
                certStore.Open(OpenFlags.ReadOnly);

                var certificates = certStore.Certificates;

                var domainCertificates = certificates.Find(X509FindType.FindBySubjectDistinguishedName, "CN=YouFancyDomain", false);

                if (domainCertificates.Count > 0)
                {
                    return domainCertificates[0]; // validate certificate with thumbprint
                }
                else
                {
                    var localhostCertificates = certificates.Find(X509FindType.FindBySubjectDistinguishedName, "CN=localhost", false);

                    return localhostCertificates.Count == 0 ? null : localhostCertificates[0];
                }
            }
            finally
            {
                certStore.Close();
            }
        }
    }
}