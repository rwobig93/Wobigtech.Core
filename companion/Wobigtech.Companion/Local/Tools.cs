using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Runtime.InteropServices;
using System.Net.Security;
using Serilog.Core;
using Serilog;
using NATS.Client;

namespace Wobigtech.Companion.Shared
{
    public static class Tools
    {
        public static bool CreateSelfSignedCert(X509ContentType certType, string certLocation, string certName)
        {
            var ecdsa = ECDsa.Create(); // generate asymmetric key pair
            var req = new CertificateRequest($"cn={Environment.MachineName}", ecdsa, HashAlgorithmName.SHA256);
            var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));
            switch (certType)
            {
                case X509ContentType.Pfx:
                    // Create PFX (PKCS #12) with private key
                    certLocation = $@"{Constants.PathConfigDefault}\LocalCompanionCert.pfx";
                    File.WriteAllBytes(certLocation, cert.Export(X509ContentType.Pfx, Constants.CertPass));
                    Constants.Config.CompanionCertLocation = certLocation;
                    return true;
                case X509ContentType.Cert:
                    // Create Base 64 encoded CER (public key only)
                    certLocation = $@"{Constants.PathConfigDefault}\LocalCompanionCert.cer";
                    Constants.Config.CompanionCertLocation = certLocation;
                    File.WriteAllText(certLocation,
                        "-----BEGIN CERTIFICATE-----\r\n"
                        + Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks)
                        + "\r\n-----END CERTIFICATE-----");
                    return true;
                default:
                    return false;
            }
        }
        public static bool PromptYesNo(string question)
        {
            Log.Debug($"Asking PromptYesNo({question})");
            bool answered = false;
            string answer = "";
            while (!answered)
            {
                Console.Write($"{question} [y/n]? ");
                answer = Console.ReadLine().ToLower();
                if (answer != "y" && answer != "n")
                {
                    Log.Debug($"Answer was invalid: {answer}");
                    Console.WriteLine("You entered an invalid response, please try again");
                }
                else
                {
                    answered = true;
                }
            }
            Log.Information($"YesNo answer was: {answer}");
            if (answer == "y")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool VerifyServerCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors, out string certMessage)
        {
            certMessage = "";
            if (sender is null)
            {
                certMessage += nameof(sender);
            }

            if (certificate is null)
            {
                certMessage += nameof(certificate);
            }

            if (chain is null)
            {
                certMessage += nameof(chain);
            }

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                certMessage += "Server cert validation failed, likely a self-signed or out of date cert, allowing";
                return true;
            }
            else
            {
                certMessage += $"SSL error: {sslPolicyErrors}";
                return true;
            }
        }
    }
}
