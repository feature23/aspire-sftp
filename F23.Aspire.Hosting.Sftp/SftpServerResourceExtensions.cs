using System.Net.Sockets;
using Aspire.Hosting.ApplicationModel;
using F23.Aspire.Hosting.Sftp;

// ReSharper disable once CheckNamespace
namespace Aspire.Hosting;

public static class SftpServerResourceExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        public IResourceBuilder<SftpServerResource> AddSftpServer(string name,
            int? port = null,
            string? volumeNamePrefix = null)
        {
            var resource = new SftpServerResource(name);

            var resourceBuilder = builder.AddResource(resource)
                .WithImage("feature23/aspire-sftp")
                .WithImageTag("latest")
                .WithImageRegistry("ghcr.io")
                .WithEndpoint(
                    targetPort: 22,
                    port: port,
                    protocol: ProtocolType.Tcp,
                    scheme: "sftp",
                    name: "sftp",
                    isExternal: true,
                    isProxied: true)
                .WithEnvironment(async context =>
                {
                    context.EnvironmentVariables["SFTP_USERS"] = await resource.GetSftpUsersStringAsync(context.CancellationToken);
                });

            // Precompute this once and also make it available to users of the resource
            resource.VolumeNamePrefix = volumeNamePrefix ?? VolumeNameGenerator.Generate(resourceBuilder, string.Empty);

            return resourceBuilder;
        }
    }

    extension(IResourceBuilder<SftpServerResource> builder)
    {
        public IResourceBuilder<SftpServerResource> WithSshHostKeysVolume(
            string? volumeName = null)
        {
            return builder.WithVolume(volumeName ?? builder.Resource.GetVolumeName("ssh-host-keys"), "/etc/ssh_volume");
        }

        public IResourceBuilder<SftpServerResource> WithSftpUser(string username,
            IResourceBuilder<ParameterResource>? password = null,
            IReadOnlyList<string>? directories = null)
        {
            var passwordParameter = password?.Resource
                                    ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(
                                        builder: builder.ApplicationBuilder,
                                        name: $"{builder.Resource.Name}-{username}-password",
                                        minLower: 1,
                                        minUpper: 1,
                                        minNumeric: 1,
                                        special: false); // to avoid possible issues with special characters in passwords, like `:` is known to break

            var user = new SftpUser(username, passwordParameter, directories);
            builder.Resource.AddUser(user);
            return builder;
        }

        public IResourceBuilder<SftpServerResource> WithSftpVolume(string username,
            string path,
            string? volumeName = null,
            bool isReadOnly = false)
        {
            return builder.WithVolume(volumeName ?? builder.Resource.GetVolumeName($"{username}-{path.Replace('/', '-')}"),
                target: $"/home/{username}/{path.TrimStart('/')}",
                isReadOnly);
        }

        public ParameterResource GetSftpUserPassword(string username)
        {
            var user = builder.Resource.Users.FirstOrDefault(u => u.Username == username);
            return user == null ? throw new ArgumentException($"SFTP user '{username}' not found in resource '{builder.Resource.Name}'.", nameof(username)) : user.Password;
        }
    }
}
