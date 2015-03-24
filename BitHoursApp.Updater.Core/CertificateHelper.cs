using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.Updater.Core
{
    public class CertificateHelper
    {
        public static bool Verify(string assemblyPath, X509Certificate2 assemblyCertificate = null)
        {
            try
            {
                var isVerified = WinTrust.VerifyEmbeddedSignature(assemblyPath, WinVerifyTrustResult.ProviderUnknown);

                X509Certificate2 cert = new X509Certificate2(assemblyPath);

                if (assemblyCertificate == null)
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    assemblyCertificate = new X509Certificate2(assembly.Location);
                }

                return isVerified && cert.Equals(assemblyCertificate);
            }
            catch
            {
            }

            return false;            
        }
    }
}