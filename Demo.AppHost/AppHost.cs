using F23.Aspire.Hosting.Sftp;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddSftpServer("sftp-server", port: 2222)
    .WithDockerfile("..") // NOTE: This line is only for testing the Dockerfile locally, remove it for real-world usage.
    .WithSshHostKeysVolume() // persist SSH host keys across restarts
    .WithSftpUser("foo", directories: ["data"]) // auto-generated password stored in user secrets
    .WithSftpVolume("foo", "data"); // persist user data across restarts

builder.Build().Run();
