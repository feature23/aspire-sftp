#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'

$latestTag = git tag --sort=-creatordate --merged HEAD | Where-Object { $_ -match '^v\d+\.\d+\.\d+$' } | Select-Object -First 1

if (-not $latestTag) {
    Write-Error "No semantic version tag found. Tags must be in format 'v1.2.3' (e.g., v1.21.0)."
    exit 1
}

$version = $latestTag.Substring(1)  # Remove leading 'v'

Write-Output $version
