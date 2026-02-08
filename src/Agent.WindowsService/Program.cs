using Agent.WindowsService;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Application;
using Agent.WindowsService.CommandLine;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;
using Agent.WindowsService.Infrastructure.Communication;
using Agent.WindowsService.Infrastructure.Executors;
using Agent.WindowsService.Infrastructure.Metric;
using Agent.WindowsService.Infrastructure.Store;
using Agent.WindowsService.Validators;
using FluentValidation;
using Serilog;
using System.Net.Security;

var options = CommandLineParser.Parse(args);
var shouldRunService = await CommandLineHandler.ProcessAsync(options);
if (!shouldRunService)
{
    return;
}

Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Information()
  .WriteTo.Console()
  .WriteTo.File(PathConfig.LogsFilePath,
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 7)
  .CreateLogger();
try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddWindowsService(serviceOptions =>
    {
      serviceOptions.ServiceName = "DCIAgentService";
    });

    builder.Services.AddSingleton<IStateMachine, StateMachine>();
    builder.Services.AddSingleton<IMetricCollector, MetricCollector>();

    builder.Services.AddTransient<IValidator<Instruction>, InstructionValidator>();
    builder.Services.AddTransient<IInstructionExecutor, ShellExecutor>();
    builder.Services.AddTransient<IInstructionExecutor, GpoExecutor>();
    builder.Services.AddTransient<IInstructionExecutor, ConfigExecutor>();

    builder.Services.AddSingleton<IConfigurationStore, JsonConfigurationStore>();
    builder.Services.AddSingleton<IMetricStore, SqliteMetricStore>();
    builder.Services.AddSingleton<IInstructionStore, SqliteInstructionStore>();

    builder.Services.AddSingleton<ICertificateStore, DpapiCertificateStore>();
    builder.Services.AddTransient<ICaEnrollmentService, CaEnrollmentService>();
    builder.Services.AddSerilog();

    builder.Services.AddHttpClient("CertificateEnrollment", client =>
    {
      client.Timeout = TimeSpan.FromSeconds(60);
      client.DefaultRequestHeaders.Add("User-Agent", "Agent.WindowsService/1.0");
    })
    .ConfigurePrimaryHttpMessageHandler(sp =>
    {
      var certStore = sp.GetRequiredService<ICertificateStore>();
      var logger = sp.GetRequiredService<ILogger<CaEnrollmentService>>();
      var isDevelopment = builder.Environment.IsDevelopment();

      var handler = new SocketsHttpHandler
      {
        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
        MaxConnectionsPerServer = 10,
        SslOptions = new SslClientAuthenticationOptions
        {
          RemoteCertificateValidationCallback = (_, cert, chain, errors) =>
          {
            if (errors is SslPolicyErrors.None)
                return true;

            // In production, only accept certificates signed by our CA
            if (errors is SslPolicyErrors.RemoteCertificateChainErrors)
            {
                try
                {
                    var caCert = certStore.GetCaCertificate();
                    if (caCert is not null && cert is not null)
                    {
                        var serverCert = cert as System.Security.Cryptography.X509Certificates.X509Certificate2
                                       ?? new System.Security.Cryptography.X509Certificates.X509Certificate2(cert);

                        // Create new chain instead of using provided one (may have null ExtraStore)
                        using var customChain = new System.Security.Cryptography.X509Certificates.X509Chain();
                        customChain.ChainPolicy.ExtraStore.Add(caCert);
                        customChain.ChainPolicy.VerificationFlags = System.Security.Cryptography.X509Certificates.X509VerificationFlags.AllowUnknownCertificateAuthority;
                        customChain.ChainPolicy.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;

                        if (customChain.Build(serverCert))
                        {
                            var chainRoot = customChain.ChainElements[^1].Certificate;
                            if (chainRoot.Thumbprint.Equals(caCert.Thumbprint, StringComparison.OrdinalIgnoreCase))
                            {
                                logger.LogDebug("Server certificate validated with internal CA");
                                return true;
                            }
                            logger.LogWarning("Certificate chain root does not match expected CA");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during certificate chain validation");
                }
                if (isDevelopment)
                {
                    logger.LogWarning("Accepting invalid server certificate in development mode");
                    return true;
                }

                logger.LogError("Server certificate validation failed in production");
                return false;
            }

            // In production, reject all other SSL errors
            if (!isDevelopment)
            {
                logger.LogError("SSL errors during certificate validation: {Errors}", errors);
                return false;
            }

            logger.LogWarning("Accepting SSL errors in development: {Errors}", errors);
            return true;
          },
          LocalCertificateSelectionCallback = (sender, targetHost, localCerts, remoteCert, acceptableIssuers) =>
          {
            var clientCert = certStore.GetClientCertificate();
            if (clientCert is not null)
            {
                logger.LogDebug("Providing client certificate: {Subject}", clientCert.Subject);
                return clientCert;
            }
            return null!;
          }
        }
      };

      var clientCert = certStore.GetClientCertificate();
      if (clientCert is null)
        return handler;

      handler.SslOptions.ClientCertificates = new System.Security.Cryptography.X509Certificates.X509CertificateCollection
      {
        clientCert
      };
      logger.LogDebug("mTLS client certificate loaded for enrollment service");
      return handler;
    });

    builder.Services.AddHttpClient("AgentServerClient", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("User-Agent", "Agent.WindowsService/1.0");
    })
    .ConfigurePrimaryHttpMessageHandler(sp =>
    {
      var logger = sp.GetRequiredService<ILogger<HttpServerClient>>();
      var certStore = sp.GetRequiredService<ICertificateStore>();
      var isDevelopment = builder.Environment.IsDevelopment();

      var handler = new SocketsHttpHandler
      {
        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
        MaxConnectionsPerServer = 10,
        SslOptions = new SslClientAuthenticationOptions
        {
          RemoteCertificateValidationCallback = (_, cert, chain, errors) =>
          {
            if (errors is SslPolicyErrors.None)
                return true;

            if (errors is SslPolicyErrors.RemoteCertificateChainErrors)
            {
                try
                {
                    var caCert = certStore.GetCaCertificate();
                    if (caCert is not null && cert is not null)
                    {
                        var serverCert = cert as System.Security.Cryptography.X509Certificates.X509Certificate2
                                       ?? new System.Security.Cryptography.X509Certificates.X509Certificate2(cert);

                        using var customChain = new System.Security.Cryptography.X509Certificates.X509Chain();
                        customChain.ChainPolicy.ExtraStore.Add(caCert);
                        customChain.ChainPolicy.VerificationFlags = System.Security.Cryptography.X509Certificates.X509VerificationFlags.AllowUnknownCertificateAuthority;
                        customChain.ChainPolicy.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;

                        if (customChain.Build(serverCert))
                        {
                            var chainRoot = customChain.ChainElements[customChain.ChainElements.Count - 1].Certificate;
                            if (chainRoot.Thumbprint.Equals(caCert.Thumbprint, StringComparison.OrdinalIgnoreCase))
                            {
                                logger.LogDebug("Server certificate validated with internal CA");
                                return true;
                            }
                            logger.LogWarning("Certificate chain root does not match expected CA");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during certificate chain validation");
                }

                if (isDevelopment)
                {
                    logger.LogWarning("Accepting self-signed server certificate in development");
                    return true;
                }

                logger.LogError("Server certificate validation failed in production");
                return false;
            }

            if (!isDevelopment)
            {
                logger.LogError("SSL errors during server certificate validation: {Errors}", errors);
                return false;
            }

            logger.LogWarning("Accepting SSL errors in development: {Errors}", errors);
            return true;
          },
          LocalCertificateSelectionCallback = (_,_,_,_,_) =>
          {
            var clientCert = certStore.GetClientCertificate();
            if (clientCert is not null)
            {
                logger.LogDebug("Providing client certificate: {Subject}", clientCert.Subject);
                return clientCert;
            }
            logger.LogDebug("No client certificate available");
            return null!;
          }
        }
      };

      var clientCert = certStore.GetClientCertificate();
      if (clientCert is null)
        return handler;

      handler.SslOptions.ClientCertificates = new System.Security.Cryptography.X509Certificates.X509CertificateCollection
      {
        clientCert
      };
      logger.LogInformation("mTLS client certificate loaded. Subject: {Subject}", clientCert.Subject);
      return handler;
    });

    builder.Services.AddTransient<IServerClient, HttpServerClient>();
    builder.Services.AddHostedService<Worker>();
    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
