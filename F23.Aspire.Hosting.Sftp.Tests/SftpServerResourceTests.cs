using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace F23.Aspire.Hosting.Sftp.Tests;

public class SftpServerResourceTests
{
    [Fact]
    public void Constructor_ShouldSetName()
    {
        // Arrange
        const string name = "my-sftp-server";

        // Act
        var resource = new SftpServerResource(name);

        // Assert
        Assert.Equal(name, resource.Name);
    }

    [Fact]
    public void AddUser_ShouldAddUserToUsersList()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resource = new SftpServerResource("test-sftp");
        var passwordParameter = builder.AddParameter("test-password", new GenerateParameterDefault());
        var user = new SftpUser("testuser", passwordParameter.Resource, ["data"]);

        // Act
        resource.AddUser(user);

        // Assert
        Assert.Contains(user, resource.Users);
    }

    [Fact]
    public void GetVolumeName_ShouldReturnCorrectVolumeName()
    {
        // Arrange
        var resource = new SftpServerResource("test-sftp")
        {
            VolumeNamePrefix = "prefix-"
        };

        // Act
        var volumeName = resource.GetVolumeName("data");

        // Assert
        Assert.Equal("prefix-data", volumeName);
    }

    [Fact]
    public async Task GetSftpUsersStringAsync_WithDirectories_ShouldReturnCorrectFormat()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resource = new SftpServerResource("test-sftp");
        var passwordParameter = builder.AddParameter("test-password", new GenerateParameterDefault());
        var user = new SftpUser("testuser", passwordParameter.Resource, ["data", "moredata"]);
        resource.AddUser(user);
        var expectedPassword = await passwordParameter.Resource.GetValueAsync(CancellationToken.None);
        var expectedString = $"testuser:{expectedPassword}:::data,moredata";

        // Act
        var usersString = await resource.GetSftpUsersStringAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedString, usersString);
    }

    [Fact]
    public async Task GetSftpUsersStringAsync_WithoutDirectories_ShouldReturnCorrectFormat()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder();
        var resource = new SftpServerResource("test-sftp");
        var passwordParameter = builder.AddParameter("test-password", new GenerateParameterDefault());
        var user = new SftpUser("testuser", passwordParameter.Resource);
        resource.AddUser(user);
        var expectedPassword = await passwordParameter.Resource.GetValueAsync(CancellationToken.None);
        var expectedString = $"testuser:{expectedPassword}";

        // Act
        var usersString = await resource.GetSftpUsersStringAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedString, usersString);
    }
}
