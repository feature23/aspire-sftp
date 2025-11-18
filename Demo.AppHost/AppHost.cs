var builder = DistributedApplication.CreateBuilder(args);

var sftp = builder.AddSftpServer("sftp-server")
    .WithDockerfile("..") // NOTE: This line is only for testing the Dockerfile locally, remove it for real-world usage.
    .WithSshHostKeysVolume() // persist SSH host keys across restarts
    .WithSftpUser(username: "foo", directories: ["data"]) // auto-generated password stored in user secrets
    .WithSftpVolume(username: "foo", path: "data"); // persist user data across restarts

// debug env vars and test sftp
builder.AddDockerfile("env-var-debug", contextPath: "../debug")
    .WaitFor(sftp)
    .WithReference(sftp.GetEndpoint("sftp"))
    .WithEnvironment("SSHPASS", sftp.GetSftpUserPassword("foo"))
    .WithEnvironment("SFTP_HOST", sftp.GetEndpoint("sftp").Property(EndpointProperty.Host))
    .WithEnvironment("SFTP_PORT", sftp.GetEndpoint("sftp").Property(EndpointProperty.Port));

builder.Build().Run();
