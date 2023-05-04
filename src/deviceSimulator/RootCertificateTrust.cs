using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using MQTTnet.Client;

namespace deviceSimulator
{

    /// <summary>
    /// Verifies certificates against a list of manually trusted certs.
    /// If a certificate is not in the Windows cert store, this will check that it's valid per our internal code.
    /// </summary>
    internal class RootCertificateTrust
    {
        X509Certificate2Collection certificates;
        internal RootCertificateTrust()
        {
            certificates = new X509Certificate2Collection();
        }

        /// <summary>
        /// Add a trusted certificate
        /// </summary>
        /// <param name="x509Certificate2"></param>
        internal void AddCert(X509Certificate2 x509Certificate2)
        {
            certificates.Add(x509Certificate2);
        }
       
        /// <summary>
        /// This matches the delegate signature expected for certificate verification for MQTTNet
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        internal bool VerifyServerCertificate(MqttClientCertificateValidationEventArgs arg) => VerifyServerCertificate(new object(), arg.Certificate, arg.Chain, arg.SslPolicyErrors);
        
        /// <summary>
        /// This matches the delegate signature expected for certificate verification for M2MQTT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        internal bool VerifyServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None) return true;

            X509Chain chainNew = new X509Chain();
            var chainTest = chain;

            chainTest.ChainPolicy.ExtraStore.AddRange(certificates);

            // Check all properties
            chainTest.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

            // This setup does not have revocation information
            chainTest.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            // Build the chain
            var buildResult = chainTest.Build(new X509Certificate2(certificate));

            //Just in case it built with trust
            if (buildResult) return true;

            //If the error is something other than UntrustedRoot, fail
            foreach (var status in chainTest.ChainStatus)
            {
                if (status.Status != X509ChainStatusFlags.UntrustedRoot)
                {
                    return false;
                }
            }

            //If the UntrustedRoot is on something OTHER than the GreenGrass CA, fail
            foreach (var chainElement in chainTest.ChainElements)
            {
                foreach (var chainStatus in chainElement.ChainElementStatus)
                {
                    if (chainStatus.Status == X509ChainStatusFlags.UntrustedRoot)
                    {
                        var found = certificates.Find(X509FindType.FindByThumbprint, chainElement.Certificate.Thumbprint, false);
                        if (found.Count == 0) return false;
                    }
                }
            }

            return true;
        }

    }
}
