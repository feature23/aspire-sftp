# Aspire SFTP Server Hosting

An Aspire hosting package that enables easy integration of SFTP servers into your app host.

## Overview

`F23.Aspire.Hosting.Sftp` provides extensions to Aspire for hosting and managing a SFTP servers in your development workflow. 
It handles Docker image management, volume persistence, user credentials, and SSH host key management.

## Acknowledgements

The Docker image used for the SFTP server is based on the excellent MIT-licensed project [atmoz/sftp](https://github.com/atmoz/sftp).
It has been modified with the following changes:

- Supports volume persistence for SSH host keys where the keys are generated on first startup if not present
- Fixes permission issues with mounted volumes by ensuring correct ownership and permissions on startup

This project is an open-source project by [feature[23]](https://feature23.com).

## Features

- **Easy SFTP Server Setup** - Add SFTP servers to your Aspire app host with a single method call
- **Automatic User Management** - Create SFTP users with auto-generated secure passwords
- **Volume Persistence** - Configure persistent volumes for user data and SSH host keys
- **Password Management** - Seamlessly integrate with Aspire's parameter resources for secure credential handling
- **Docker-based** - Uses containerized SFTP servers for easy startup and isolation

## Installation

Install the NuGet package:

```bash
dotnet add package F23.Aspire.Hosting.Sftp
```

## Quick Start

Add the following to your Aspire `AppHost.cs`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var sftp = builder.AddSftpServer("sftp-server")
    // Persist SSH host keys across restarts:
    .WithSshHostKeysVolume()
    // Create user with auto-generated password:
    .WithSftpUser(username: "foo", directories: ["data"])
    // Create persistent volume for user data 
    .WithSftpVolume(username: "foo", path: "data");

builder.Build().Run();
```

### Passing Credentials to Other Services

```csharp
var backupService = builder.AddDockerfile("backup-service")
    .WaitFor(sftp)
    .WithReference(sftp.GetEndpoint("sftp"))
    .WithEnvironment("SFTP_USER", "backup-user")
    .WithEnvironment("SFTP_PASSWORD", sftp.GetSftpUserPassword("backup-user"));
```

## API Reference

### `AddSftpServer(name, port, volumeNamePrefix)`

Adds an SFTP server resource to your application.

**Parameters:**
- `name` (string) - The logical name of the SFTP resource
- `port` (int?, optional) - The port to expose the SFTP server on. If not specified, a random port is assigned
- `volumeNamePrefix` (string?, optional) - Prefix for generated Docker volume names

**Returns:** `IResourceBuilder<SftpServerResource>` for further configuration

**Example:**
```csharp
var sftp = builder.AddSftpServer("sftp-server", port: 2222);
```

### `WithSshHostKeysVolume(volumeName)`

Configures SSH host key persistence across container restarts.
It is strongly recommended to use this, so that you do not get SSH host key warnings when restarting the app host.

**Parameters:**
- `volumeName` (string?, optional) - Custom Docker volume name. If not specified, one is auto-generated

**Example:**
```csharp
sftp.WithSshHostKeysVolume();
```

### `WithSftpUser(username, password, directories)`

Creates an SFTP user account.

**Parameters:**
- `username` (string) - The username for the SFTP account
- `password` (IResourceBuilder<ParameterResource>?, optional) - Custom password parameter. If not specified, a secure password is auto-generated
- `directories` (IReadOnlyList<string>?, optional) - List of directories the user has access to in the user's home directory

**Example:**
```csharp
sftp.WithSftpUser("foo", directories: ["data", "uploads"]);
```

### `WithSftpVolume(username, path, volumeName, isReadOnly)`

Configures a persistent volume for a user's directory.

**Parameters:**
- `username` (string) - The SFTP username
- `path` (string) - The path within the user's home directory (e.g., "data", "uploads")
- `volumeName` (string?, optional) - Custom Docker volume name. If not specified, one is auto-generated
- `isReadOnly` (bool, default: false) - Whether the Docker volume should be mounted as read-only

**Example:**
```csharp
sftp.WithSftpVolume("foo", "data");
sftp.WithSftpVolume("admin", "backups", isReadOnly: true);
```

### `GetSftpUserPassword(username)`

Retrieves the password parameter for a specific SFTP user. Useful for passing credentials to other services.

**Parameters:**
- `username` (string) - The username to retrieve the password for

**Returns:** `ParameterResource` containing the password

**Example:**
```csharp
var fooPassword = sftp.GetSftpUserPassword("foo");
```

## Advanced Usage

### Custom Passwords

```csharp
var customPassword = builder.AddParameter("my-sftp-password", secret: true);

var sftp = builder.AddSftpServer("sftp-server")
    .WithSftpUser("admin", password: customPassword);
```

## License

MIT - See LICENSE file for details
