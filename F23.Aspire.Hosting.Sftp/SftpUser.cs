using Aspire.Hosting.ApplicationModel;

namespace F23.Aspire.Hosting.Sftp;

public record SftpUser(string Username, ParameterResource Password, IReadOnlyList<string>? Directories = null);
