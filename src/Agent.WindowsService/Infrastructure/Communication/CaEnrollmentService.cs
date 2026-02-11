using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Config;

namespace Agent.WindowsService.Infrastructure.Communication;

public class CaEnrollmentService(
  ILogger<CaEnrollmentService> logger,
  ICertificateStore certificateStore,
  IHttpClientFactory httpClientFactory
) : ICaEnrollmentService {

  private readonly HttpClient _httpClient = httpClientFactory.CreateClient("CertificateEnrollment");

  public async Task<bool> EnrollWithTokenAsync(
    string serverUrl,
    string agentName,
    string enrollmentToken,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Starting token-based certificate enrollment for agent: {AgentName}", agentName);
    try
    {
      var (csrPem, privateKey) = GenerateCsr(agentName);
      using (privateKey)
      {
        var request = new
        {
          AgentName = agentName,
          EnrollmentToken = enrollmentToken,
          CsrPem = csrPem
        };

        var url = UrlConfig.PostEnrollmentUrl(serverUrl);
        logger.LogDebug("Enrollment URL: {Url}", url);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
          var error = await response.Content.ReadAsStringAsync(cancellationToken);
          logger.LogError(
            "Token enrollment failed with status {Status}: {Error}",
            response.StatusCode,
            error);
          return false;
        }

        var enrollmentResponse = await response.Content
          .ReadFromJsonAsync<CertificateEnrollmentResponse>(
          cancellationToken: cancellationToken);
        if (enrollmentResponse is null)
        {
          logger.LogError("Invalid enrollment response from server");
          return false;
        }

        await certificateStore.StoreCertificateAsync(
          enrollmentResponse.CertificatePem,
          privateKey,
          cancellationToken);

        await certificateStore.StoreCaCertificateAsync(
          enrollmentResponse.CaCertificatePem,
          cancellationToken);

        logger.LogInformation(
          "Token-based enrollment successful. Expires: {Expiry}",
          enrollmentResponse.ExpiresAt);

        return true;
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Token-based enrollment failed");
      return false;
    }
  }

  public async Task<bool> RenewAsync(
    string serverUrl,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Starting certificate renewal");

    try
    {
      var currentCert = certificateStore.GetClientCertificate();
      if (currentCert is null)
      {
        logger.LogError("No current certificate found for renewal");
        return false;
      }

      var agentName = currentCert.GetNameInfo(X509NameType.SimpleName, false);
      var (csrPem, privateKey) = GenerateCsr(agentName);
      using (privateKey)
      {
        var request = new
        {
          CsrPem = csrPem
        };

        var url = UrlConfig.PostRenewalUrl(serverUrl);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
          var error = await response.Content.ReadAsStringAsync(cancellationToken);
          logger.LogError(
            "Certificate renewal failed with status {Status}: {Error}",
            response.StatusCode,
            error);
          return false;
        }

        var renewalResponse = await response.Content.ReadFromJsonAsync<CertificateEnrollmentResponse>(
          cancellationToken: cancellationToken);
        if (renewalResponse is null)
        {
          logger.LogError("Invalid renewal response from server");
          return false;
        }

        await certificateStore.StoreCertificateAsync(
          renewalResponse.CertificatePem,
          privateKey,
          cancellationToken);

        logger.LogInformation(
          "Certificate renewal successful. New expiry: {Expiry}",
          renewalResponse.ExpiresAt);

        return true;
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Certificate renewal failed");
      return false;
    }
  }

  public async Task<bool> IsCertificateRevokedAsync(
    string serverUrl,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Checking if the certificate is revoked");
    try
    {
      var currentCert = certificateStore.GetClientCertificate();
      if (currentCert is null)
      {
        logger.LogWarning("No current certificate found to check revocation status");
        return false;
      }

      var thumbprint = currentCert.Thumbprint;
      var url = UrlConfig.GetRevocationCheckUrl(serverUrl, thumbprint);
      logger.LogDebug("Revocation check URL: {Url}", url);

      using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
      var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
      if (!response.IsSuccessStatusCode)
      {
        logger.LogError("Failed to check revocation status. Status: {Status}", response.StatusCode);
        return false;
      }

      var revocationResponse = await response.Content
        .ReadFromJsonAsync<CertRevocationResponse>(
        cancellationToken: cancellationToken);
      if (revocationResponse is null)
      {
        logger.LogError("Invalid revocation response from server");
        return false;
      }

      logger.LogInformation(
        "Certificate revocation status: {IsRevoked}, RevokedAt: {RevokedAt}",
        revocationResponse.IsRevoked,
        revocationResponse.RevokedAt);

      return revocationResponse.IsRevoked;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to check certificate revocation status");
      return false;
    }
  }

  private (string CsrPem, RSA PrivateKey) GenerateCsr(string agentName)
  {
    var rsa = RSA.Create(2048);
    var subjectName = new X500DistinguishedName($"CN={agentName}");
    var request = new CertificateRequest(
      subjectName,
      rsa,
      HashAlgorithmName.SHA256,
      RSASignaturePadding.Pkcs1);

    request.CertificateExtensions.Add(
      new X509KeyUsageExtension(
        X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
        true));
    request.CertificateExtensions.Add(
      new X509EnhancedKeyUsageExtension(
        [new Oid("1.3.6.1.5.5.7.3.2")],
        true));

    var csrDer = request.CreateSigningRequest();
    var csrPem = new StringBuilder();
    csrPem.AppendLine("-----BEGIN CERTIFICATE REQUEST-----");
    csrPem.AppendLine(Convert.ToBase64String(csrDer, Base64FormattingOptions.InsertLineBreaks));
    csrPem.AppendLine("-----END CERTIFICATE REQUEST-----");

    logger.LogDebug("Generated CSR for {AgentName}", agentName);
    return (csrPem.ToString(), rsa);
  }
}

/// <summary>
/// Response from certificate enrollment/renewal endpoints.
/// </summary>
internal record CertificateEnrollmentResponse(
  string CertificatePem,
  string CaCertificatePem,
  DateTimeOffset ExpiresAt);

/// <summary>
/// Response from certificate revocation check endpoint.
/// </summary>
internal record CertRevocationResponse(
  bool IsRevoked,
  DateTimeOffset? RevokedAt,
  string? Reason);

