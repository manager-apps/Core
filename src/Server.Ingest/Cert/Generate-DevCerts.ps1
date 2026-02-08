#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates CA and Server certificates for development mTLS.
.DESCRIPTION
    Creates ca.pfx (Certificate Authority) and server.pfx (Server HTTPS) in the Cert folder.
#>

param(
    [string]$OutputPath = ".",
    [string]$CaCommonName = "Manager Internal CA",
    [string]$ServerCommonName = "localhost"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Generating mTLS Certificates ===" -ForegroundColor Cyan

# Create CA certificate
Write-Host "Creating CA certificate..." -ForegroundColor Yellow

$caParams = @{
    Subject           = "CN=$CaCommonName, O=Manager, OU=mTLS"
    KeyAlgorithm      = "RSA"
    KeyLength         = 4096
    KeyUsage          = "CertSign", "CRLSign", "DigitalSignature"
    KeyExportPolicy   = "Exportable"
    NotAfter          = (Get-Date).AddYears(10)
    CertStoreLocation = "Cert:\CurrentUser\My"
    TextExtension     = @("2.5.29.19={critical}{text}CA=true&pathlength=1")
}

$caCert = New-SelfSignedCertificate @caParams
Write-Host "  CA Subject: $($caCert.Subject)" -ForegroundColor Green
Write-Host "  CA Thumbprint: $($caCert.Thumbprint)" -ForegroundColor Green

# Create Server certificate (signed by CA)
Write-Host "Creating Server certificate..." -ForegroundColor Yellow

$serverParams = @{
    Subject           = "CN=$ServerCommonName, O=Manager"
    KeyAlgorithm      = "RSA"
    KeyLength         = 2048
    KeyUsage          = "DigitalSignature", "KeyEncipherment"
    KeyExportPolicy   = "Exportable"
    NotAfter          = (Get-Date).AddYears(2)
    CertStoreLocation = "Cert:\CurrentUser\My"
    Signer            = $caCert
    TextExtension     = @(
        "2.5.29.37={text}1.3.6.1.5.5.7.3.1"
        "2.5.29.17={text}dns=localhost&dns=127.0.0.1"
    )
}

$serverCert = New-SelfSignedCertificate @serverParams
Write-Host "  Server Subject: $($serverCert.Subject)" -ForegroundColor Green
Write-Host "  Server Thumbprint: $($serverCert.Thumbprint)" -ForegroundColor Green

# Export certificates (simple password for dev - change for production!)
$devPassword = (ConvertTo-SecureString -String "dev" -Force -AsPlainText)

$caPath = Join-Path $OutputPath "ca.pfx"
$serverPath = Join-Path $OutputPath "server.pfx"

Export-PfxCertificate -Cert $caCert -FilePath $caPath -Password $devPassword -Force | Out-Null
Export-PfxCertificate -Cert $serverCert -FilePath $serverPath -Password $devPassword -ChainOption BuildChain -Force | Out-Null

Write-Host ""
Write-Host "Exported:" -ForegroundColor Green
Write-Host "  $caPath" -ForegroundColor White
Write-Host "  $serverPath" -ForegroundColor White

# Cleanup from cert store
Remove-Item "Cert:\CurrentUser\My\$($caCert.Thumbprint)" -Force
Remove-Item "Cert:\CurrentUser\My\$($serverCert.Thumbprint)" -Force

Write-Host ""
Write-Host "Done! CA and Server certificates created." -ForegroundColor Cyan
