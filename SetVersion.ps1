#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'

# Get version from GetVersion.ps1
$version = ./GetVersion.ps1

Write-Host "Setting version: $version"

# Requires the dotnet-setversion tool installed:
#   dotnet tool install -g dotnet-setversion
setversion $version F23.Aspire.Hosting.Sftp/F23.Aspire.Hosting.Sftp.csproj

Write-Host "Version set successfully"
