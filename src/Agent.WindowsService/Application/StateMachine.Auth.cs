using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Application;

public partial class StateMachine
{
  private async Task HandleAuthenticationEntryAsync()
  {
    _logger.LogInformation("Entering Authentication state");
    try
    {
      var config = await _configStore.GetAsync(Token);
      if (_certificateStore.HasValidCertificate())
      {
        if (await _icaEnrollmentService.IsCertificateRevokedAsync(config.ServerUrl, Token))
        {
          _logger.LogWarning("Certificate is revoked. Transitioning to re-enrollment state.");
          await ReEnrollCertificateAsync(config);
          return;
        }

        if (_certificateStore.NeedsRenewal())
        {
          _logger.LogInformation("Certificate expires soon, initiating renewal");
          await RenewCertificateAsync(config);
        }
        else
        {
          var expiry = _certificateStore.GetCertificateExpiry();
          _logger.LogInformation("Valid certificate found, expires: {Expiry}", expiry);
        }

        _logger.LogInformation("Authentication state completed successfully (mTLS)");
        await _machine.FireAsync(Triggers.AuthSuccess);
        return;
      }

      // No valid certificate, so we need to enroll using enrollment token
      _logger.LogInformation("No valid certificate found, initiating enrollment");
      if (string.IsNullOrEmpty(config.EnrollmentToken))
      {
        _logger.LogError("Enrollment token is missing. Cannot proceed with enrollment.");
        await _machine.FireAsync(Triggers.AuthFailure);
        return;
      }

      var enrollmentSuccess = await _icaEnrollmentService.EnrollWithTokenAsync(
        config.ServerUrl,
        config.AgentName,
        config.EnrollmentToken,
        Token);
      if (!enrollmentSuccess)
      {
        _logger.LogError("Certificate enrollment failed");
        await _machine.FireAsync(Triggers.AuthFailure);
        return;
      }

      _logger.LogInformation("Certificate enrollment completed successfully");

      var updatedConfig = config with { EnrollmentToken = null };
      await _configStore.SaveAsync(updatedConfig, Token);
      if (!_certificateStore.HasValidCertificate())
      {
        _logger.LogError("No valid certificate after enrollment attempt");
        await _machine.FireAsync(Triggers.AuthFailure);
        return;
      }

      _logger.LogInformation("Authentication state completed successfully (mTLS)");
      await _machine.FireAsync(Triggers.AuthSuccess);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Authentication state cancelled");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Authentication state failed");
      await _machine.FireAsync(Triggers.AuthFailure);
    }
  }

  private async Task HandleAuthenticationExitAsync()
  {
    _logger.LogInformation("Exiting Authentication state");
    try
    {
      var config = await _configStore.GetAsync(Token);
      await Task.Delay(TimeSpan.FromSeconds(config.AuthenticationExitIntervalSeconds), Token);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Authentication state exit delay cancelled");
    }
  }

  private async Task RenewCertificateAsync(Configuration config)
  {
    try
    {
      var success = await _icaEnrollmentService.RenewAsync(
        config.ServerUrl,
        Token);
      if (success)
      {
        _logger.LogInformation("Certificate renewal completed successfully");
      }
      else
      {
        _logger.LogWarning("Certificate renewal failed, will retry later");
      }
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Certificate renewal failed");
    }
  }

  private async Task ReEnrollCertificateAsync(Configuration config)
  {
    try
    {
      _logger.LogInformation("Re-enrolling certificate for agent: {AgentName}", config.AgentName);
      if (string.IsNullOrEmpty(config.EnrollmentToken))
      {
        _logger.LogError("Cannot re-enroll: enrollment token is missing");
        await _machine.FireAsync(Triggers.AuthFailure);
        return;
      }

      await _certificateStore.DeleteCertificateAsync(Token);

      var enrollmentSuccess = await _icaEnrollmentService.EnrollWithTokenAsync(
        config.ServerUrl,
        config.AgentName,
        config.EnrollmentToken,
        Token);

      if (enrollmentSuccess)
      {
        _logger.LogInformation("Re-enrollment completed successfully");
        await _machine.FireAsync(Triggers.AuthSuccess);
      }
      else
      {
        _logger.LogError("Re-enrollment failed");
        await _machine.FireAsync(Triggers.AuthFailure);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Re-enrollment process failed");
      await _machine.FireAsync(Triggers.AuthFailure);
    }
  }
}
