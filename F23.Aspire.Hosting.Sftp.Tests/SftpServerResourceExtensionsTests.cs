using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace F23.Aspire.Hosting.Sftp.Tests;

public class SftpServerResourceExtensionsTests
{
    [Fact]
    public async Task AddSftpServer_ShouldAddResourceToBuilder()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        const string resourceName = "my-sftp-server";

        // Act
        var resource = builder.AddSftpServer(resourceName);

        // Assert
        Assert.NotNull(resource);
        Assert.Equal(resourceName, resource.Resource.Name);
        Assert.Contains(resource.Resource, builder.Resources.ToList());

        var endpoint = resource.Resource.GetEndpoint("sftp");
        Assert.NotNull(endpoint);
        Assert.Equal(22, endpoint.TargetPort);
        Assert.Equal("sftp", endpoint.Scheme);

        var envVars = await resource.Resource.GetEnvironmentVariableValuesAsync();
        Assert.True(envVars.ContainsKey("SFTP_USERS"));

        Assert.False(string.IsNullOrEmpty(resource.Resource.VolumeNamePrefix));
    }

    [Fact]
    public void WithSshHostKeysVolume_ShouldAddVolumeToResource()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddSftpServer("my-sftp-server");

        // Act
        var updatedBuilder = resourceBuilder.WithSshHostKeysVolume();

        // Assert
        Assert.NotNull(updatedBuilder);

        if (!updatedBuilder.Resource.TryGetAnnotationsOfType<ContainerMountAnnotation>(out var mountAnnotations))
        {
            Assert.Fail("No mount annotations found on the resource.");
        }

        var mount = mountAnnotations.FirstOrDefault(ma => ma.Target == "/etc/ssh_volume");
        Assert.NotNull(mount);
        Assert.StartsWith(updatedBuilder.Resource.VolumeNamePrefix, mount!.Source);
    }

    [Fact]
    public void WithSftpUser_ShouldAddUserToResource()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddSftpServer("my-sftp-server");
        const string username = "testuser";

        // Act
        var updatedBuilder = resourceBuilder.WithSftpUser(username);

        // Assert
        Assert.NotNull(updatedBuilder);
        var user = updatedBuilder.Resource.Users.FirstOrDefault(u => u.Username == username);
        Assert.NotNull(user);
    }

    [Fact]
    public void WithSftpVolume_ShouldAddVolumeToResource()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddSftpServer("my-sftp-server");
        const string username = "testuser";
        const string path = "data";

        // Act
        var updatedBuilder = resourceBuilder.WithSftpVolume(username, path);

        // Assert
        Assert.NotNull(updatedBuilder);

        if (!updatedBuilder.Resource.TryGetAnnotationsOfType<ContainerMountAnnotation>(out var mountAnnotations))
        {
            Assert.Fail("No mount annotations found on the resource.");
        }

        var mount = mountAnnotations.FirstOrDefault(ma => ma.Target == $"/home/{username}/{path}");
        Assert.NotNull(mount);
        Assert.StartsWith(updatedBuilder.Resource.VolumeNamePrefix, mount!.Source);
    }

    [Fact]
    public async Task GetSftpUserPassword_ShouldReturnCorrectPasswordParameter()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddSftpServer("my-sftp-server");
        const string username = "testuser";
        var passwordParameter = builder.AddParameter("test-password", new GenerateParameterDefault());
        resourceBuilder.WithSftpUser(username, passwordParameter);
        var expectedPassword = await passwordParameter.Resource.GetValueAsync(CancellationToken.None);

        // Act
        var retrievedPasswordParameter = resourceBuilder.GetSftpUserPassword(username);

        // Assert
        Assert.NotNull(retrievedPasswordParameter);
        Assert.Equal(passwordParameter.Resource, retrievedPasswordParameter);
        var retrievedPassword = await retrievedPasswordParameter.GetValueAsync(CancellationToken.None);
        Assert.Equal(expectedPassword, retrievedPassword);
    }
}
