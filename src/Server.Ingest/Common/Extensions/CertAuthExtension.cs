using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.Extensions.Options;
using Server.Ingest.Common.Interfaces;

namespace Server.Ingest.Common.Extensions;

internal static class CertAuthExtension
{
    internal static void AddCertificateAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
            .AddCertificate();

        services.AddSingleton<IPostConfigureOptions<CertificateAuthenticationOptions>, ConfigureCertificateOptions>();
    }
}

internal sealed class ConfigureCertificateOptions(ICaAuthority caAuthority)
    : IPostConfigureOptions<CertificateAuthenticationOptions>
{
    public void PostConfigure(string? name, CertificateAuthenticationOptions options)
    {
        var caCertPem = caAuthority.GetCaCertificatePem();
        var caCert = X509Certificate2.CreateFromPem(caCertPem);
        options.AllowedCertificateTypes = CertificateTypes.All;
        options.RevocationMode = X509RevocationMode.NoCheck;
        options.ValidateCertificateUse = false;
        options.ValidateValidityPeriod = true;
        options.ChainTrustValidationMode = X509ChainTrustMode.CustomRootTrust;
        options.CustomTrustStore = [caCert];

        options.Events = new CertificateAuthenticationEvents
        {
            OnCertificateValidated = OnCertificateValidatedAsync,
            OnAuthenticationFailed = OnAuthenticationFailedAsync
        };
    }

    private static async Task OnCertificateValidatedAsync(CertificateValidatedContext context)
    {
        var certService = context.HttpContext.RequestServices.GetService<ICertService>();
        var caAuthority = context.HttpContext.RequestServices.GetService<ICaAuthority>();
        if (certService is null || caAuthority is null)
        {
            context.Fail("Certificate validation service not available");
            return;
        }

        var clientCert = context.ClientCertificate;
        if (!caAuthority.ValidateCertificate(clientCert))
        {
            context.Fail("Certificate was not issued by trusted CA");
            return;
        }

        var agentName = clientCert.GetNameInfo(X509NameType.SimpleName, false);
        var isValid = await certService.ValidateCertificateAsync(
            clientCert.Thumbprint,
            agentName,
            context.HttpContext.RequestAborted);
        if (!isValid)
        {
            context.Fail("Certificate not recognized or revoked");
            return;
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, agentName),
            new Claim(ClaimTypes.NameIdentifier, clientCert.Thumbprint),
            new Claim("CertificateSerialNumber", clientCert.SerialNumber),
            new Claim(ClaimTypes.AuthenticationMethod, "mTLS")
        };
        var identity = new ClaimsIdentity(claims, CertificateAuthenticationDefaults.AuthenticationScheme);
        context.Principal = new ClaimsPrincipal(identity);
        context.Success();
    }

    private static Task OnAuthenticationFailedAsync(CertificateAuthenticationFailedContext context)
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILogger<CertificateAuthenticationEvents>>();

        logger.LogWarning(
            context.Exception,
            "Certificate authentication failed: {Message}",
            context.Exception?.Message ?? "Unknown error");

        return Task.CompletedTask;
    }
}
