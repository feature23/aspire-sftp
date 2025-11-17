using System.Net.Sockets;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace F23.Aspire.Hosting.Sftp;

public static class SftpServerResourceExtensions
{
    public static IResourceBuilder<SftpServerResource> AddSftpServer(this IDistributedApplicationBuilder builder,
        string name,
        int? port)
    {
        var resource = new SftpServerResource(name);

        return builder.AddResource(resource)
            .WithImage("feature23/aspire-sftp")
            .WithImageTag("latest")
            .WithImageRegistry("ghcr.io")
            .WithEndpoint(
                targetPort: 22,
                port: port,
                protocol: ProtocolType.Tcp,
                name: "sftp");
    }
}
