using F23.Aspire.Hosting.Sftp;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddSftpServer("sftp-server", port: 2222);

builder.Build().Run();
