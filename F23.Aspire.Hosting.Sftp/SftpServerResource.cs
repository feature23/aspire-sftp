using System.Text;
using Aspire.Hosting.ApplicationModel;

namespace F23.Aspire.Hosting.Sftp;

public class SftpServerResource(string name) : ContainerResource(name)
{
    private readonly List<SftpUser> _users = [];

    public void AddUser(SftpUser user) => _users.Add(user);

    public IReadOnlyList<SftpUser> Users => _users;

    public string VolumeNamePrefix { get; internal set; } = ""; // NOTE: this will be overwritten by the extension method

    public string GetVolumeName(string suffix) => $"{VolumeNamePrefix.TrimEnd('-')}-{suffix}";

    public async Task<string> GetSftpUsersStringAsync(CancellationToken cancellationToken)
    {
        var usersString = new StringBuilder();

        foreach (var user in _users)
        {
            if (usersString.Length > 0)
            {
                usersString.Append(' ');
            }

            var password = await user.Password.GetValueAsync(cancellationToken);
            usersString.Append($"{user.Username}:{password}");

            if (user.Directories is not null)
            {
                usersString.Append(":::"); // skip past uid/gid
                usersString.Append(string.Join(',', user.Directories));
            }
        }

        return usersString.ToString();
    }
}
