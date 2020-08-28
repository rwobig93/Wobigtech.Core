using CertificateManager;
using CertificateManager.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Wobigtech.Core.Comm;

namespace Wobigtech.Core.Crypto
{
    public static class Certs
    {
        private static string certPass = "WT789321";

        public static Options GetNATConnOptions(string url, string certPath, RemoteCertificateValidationCallback callback = null)
        {
            string certPathValid = ValidateCert(certPath, url);

            Options opts = ConnectionFactory.GetDefaultOptions();
            opts.Secure = true;
            opts.AddCertificate(new X509Certificate2(certPathValid, certPass));
            if (callback == null)
            {
                callback = VerifyServerCert;
            }
            opts.TLSRemoteCertificationValidationCallback = callback;
            opts.Url = $"nats://{url}";

            return opts;
        }

        public static bool VerifyServerCert(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            Log.Information($"SSL Policy Error: {sslPolicyErrors}");
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                Log.Information("No SSL Policy Errors, moving on");
                return true;
            }
            else
            {
                Log.Warning($"SSL Policy Error Detected, bypassing: {sslPolicyErrors}");
                return true;
            }
        }

        private static string ValidateCert(string certPath, string url = "localhost")
        {
            if (string.IsNullOrWhiteSpace(certPath))
            {
                //CreateSelfSignedCert(X509ContentType.Pfx, certPath, "localhost", out certPath);
                certPath = CreateSSRSACertificate(url, certPath);
            }
            else if (!File.Exists($@"{certPath}\{url.Replace(" ", "_")}.pfx"))
            {
                //CreateSelfSignedCert(X509ContentType.Pfx, certPath, "localhost", out certPath);
                certPath = CreateSSRSACertificate(url, certPath);
            }

            return certPath;
        }

        public static bool CreateSelfSignedCert(X509ContentType certType, string savePath, string hostURL, out string certPath)
        {
            var provider = new ServiceCollection()
                .AddCertificateManager()
                .BuildServiceProvider();

            var certHandler = provider.GetService<CreateCertificatesClientServerAuth>();

            var rootCA = certHandler.NewRootCertificate(
                new DistinguishedName { CommonName = "Wobigtech Root CA SS", Country = "foo" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                3, hostURL);
            rootCA.FriendlyName = "Wobigtech Root CA SS";

            var intermediateCA = certHandler.NewIntermediateChainedCertificate(
                new DistinguishedName { CommonName = "Wobigtech Intermediate CA SS", Country = "foo" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                2, hostURL, rootCA);
            intermediateCA.FriendlyName = "Wobigtech Intermediate CA SS";

            var serverCert = certHandler.NewServerChainedCertificate(
                new DistinguishedName { CommonName = hostURL, Country = "foo" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                hostURL, intermediateCA);
            serverCert.FriendlyName = $"{hostURL} Self-Signed Cert";

            var certDealer = provider.GetService<ImportExportCertificate>();

            try
            {
                switch (certType)
                {
                    case X509ContentType.Cert:
                        var rootKey = certDealer.ExportCertificatePublicKey(rootCA);
                        var rootkeyBytes = rootKey.Export(X509ContentType.Cert);
                        File.WriteAllBytes($@"{savePath}\{rootCA.FriendlyName.Replace(" ", "_")}.cer", rootkeyBytes);

                        var intermediateKey = certDealer.ExportCertificatePublicKey(intermediateCA);
                        var intermediateKeyBytes = intermediateKey.Export(X509ContentType.Cert);
                        File.WriteAllBytes($@"{savePath}\{intermediateCA.FriendlyName.Replace(" ", "_")}.cer", intermediateKeyBytes);

                        var serverKey = certDealer.ExportCertificatePublicKey(serverCert);
                        var serverKeyBytes = serverKey.Export(X509ContentType.Cert);
                        File.WriteAllBytes($@"{savePath}\{serverCert.FriendlyName.Replace(" ", "_")}.cer", serverKeyBytes);

                        certPath = $@"{savePath}\{serverCert.FriendlyName.Replace(" ", "_")}.cer";
                        return true;
                    case X509ContentType.Pfx:
                        var rootPFX = certDealer.ExportRootPfx(certPass, rootCA);
                        File.WriteAllBytes($@"{savePath}\{rootCA.FriendlyName.Replace(" ", "_")}.pfx", rootPFX);

                        var intermediatePFX = certDealer.ExportChainedCertificatePfx(certPass, intermediateCA, rootCA);
                        File.WriteAllBytes($@"{savePath}\{intermediateCA.FriendlyName.Replace(" ", "_")}.pfx", intermediatePFX);

                        var serverPFX = certDealer.ExportChainedCertificatePfx(certPass, serverCert, intermediateCA);
                        File.WriteAllBytes($@"{savePath}\{serverCert.FriendlyName.Replace(" ", "_")}.pfx", serverPFX);

                        certPath = $@"{savePath}\{serverCert.FriendlyName.Replace(" ", "_")}.pfx";
                        return true;
                    default:
                        certPath = "Cert requested wasn't of a valid type";
                        return false;
                }
            }
            catch (Exception ex)
            {
                certPath = ex.Message;
                return false;
            }
        }

        public static string CreateSSRSACertificate(string url, string certPath, SubjectAlternativeName subjectAlternativeNames = null)
        {
            string certPathFull = $@"{certPath}\{url.Replace(" ", "_")}.pfx";

            var provider = new ServiceCollection()
                .AddCertificateManager()
                .BuildServiceProvider();

            var certHandler = provider.GetService<CreateCertificates>();

            var basicConstraints = new BasicConstraints
            {
                CertificateAuthority = false,
                HasPathLengthConstraint = false,
                PathLengthConstraint = 0,
                Critical = false
            };

            if (null == subjectAlternativeNames)
            {
                subjectAlternativeNames = new SubjectAlternativeName
                {
                    DnsName = new List<string>
                    {
                        url,
                        "localhost",
                        NetworkGeneral.GetLocalIpAddress(),
                        NetworkGeneral.GetPublicIP()
                    }
                };
            }

            var x509KeyUsageFlags = X509KeyUsageFlags.DigitalSignature;

            var enhancedKeyUsages = new OidCollection
            {
                new Oid("1.3.6.1.5.5.7.3.1"),  // TLS Server auth
                new Oid("1.3.6.1.5.5.7.3.2"),  // TLS Client auth
            };

            var certificate = certHandler.NewRsaSelfSignedCertificate(
                new DistinguishedName { CommonName = url },
                basicConstraints,
                new ValidityPeriod
                {
                    ValidFrom = DateTimeOffset.UtcNow,
                    ValidTo = DateTimeOffset.UtcNow.AddYears(10)
                },
                subjectAlternativeNames,
                enhancedKeyUsages,
                x509KeyUsageFlags,
                new RsaConfiguration { KeySize = 2048 }
            );

            var certDealer = provider.GetService<ImportExportCertificate>();

            var certInBytes = certDealer.ExportSelfSignedCertificatePfx(certPass, certificate);
            File.WriteAllBytes(certPathFull, certInBytes);

            return certPathFull;
        }

        public static string CreateSSECDCertificate(string url, string certPath, SubjectAlternativeName subjectAlternativeNames = null)
        {
            string certPathFull = $@"{certPath}\{url.Replace(" ", "_")}.pfx";
            
            var provider = new ServiceCollection()
                .AddCertificateManager()
                .BuildServiceProvider();

            var certHandler = provider.GetService<CreateCertificates>();

            var basicConstraints = new BasicConstraints
            {
                CertificateAuthority = false,
                HasPathLengthConstraint = false,
                PathLengthConstraint = 0,
                Critical = false
            };

            if (null == subjectAlternativeNames)
            {
                subjectAlternativeNames = new SubjectAlternativeName
                {
                    DnsName = new List<string>
                    {
                        url,
                        "localhost",
                        NetworkGeneral.GetLocalIpAddress(),
                        NetworkGeneral.GetPublicIP()
                    }
                };
            }

            var x509KeyUsageFlags = X509KeyUsageFlags.DigitalSignature;

            var enhancedKeyUsages = new OidCollection
            {
                new Oid("1.3.6.1.5.5.7.3.1"),  // TLS Server auth
                new Oid("1.3.6.1.5.5.7.3.2"),  // TLS Client auth
            };

            var certificate = certHandler.NewRsaSelfSignedCertificate(
                new DistinguishedName { CommonName = url },
                basicConstraints,
                new ValidityPeriod
                {
                    ValidFrom = DateTimeOffset.UtcNow,
                    ValidTo = DateTimeOffset.UtcNow.AddYears(10)
                },
                subjectAlternativeNames,
                enhancedKeyUsages,
                x509KeyUsageFlags,
                new RsaConfiguration { KeySize = 2048 }
            );

            var certDealer = provider.GetService<ImportExportCertificate>();

            var certInBytes = certDealer.ExportSelfSignedCertificatePfx(certPass, certificate);
            File.WriteAllBytes(certPathFull, certInBytes);

            return certPathFull;
        }
    }
}
