using MiddlewareEngine.Models;
using Renci.SshNet;
using System.Text;

namespace MiddlewareEngine.Executors;

public class SshExecutor : IFunctionExecutor
{
    private readonly ILogger<SshExecutor> _logger;

    public SshExecutor(ILogger<SshExecutor> logger)
    {
        _logger = logger;
    }

    public async Task<FunctionExecutionResponse> ExecuteAsync(
        FunctionDefinition definition,
        Dictionary<string, object>? parameters)
    {
        try
        {
            var config = definition.ExecutionConfig;
            var host = config.SshHost ?? throw new ArgumentException("SSH host is required");
            var username = config.SshUsername ?? throw new ArgumentException("SSH username is required");
            var port = config.SshPort ?? 22;

            Renci.SshNet.ConnectionInfo connectionInfo;

            if (!string.IsNullOrEmpty(config.SshKeyPath) && File.Exists(config.SshKeyPath))
            {
                // SSH Key authentication
                var keyFile = new PrivateKeyFile(config.SshKeyPath);
                connectionInfo = new Renci.SshNet.ConnectionInfo(host, port, username, new PrivateKeyAuthenticationMethod(username, keyFile));
            }
            else if (!string.IsNullOrEmpty(config.SshPassword))
            {
                // Password authentication
                connectionInfo = new Renci.SshNet.ConnectionInfo(host, port, username, new PasswordAuthenticationMethod(username, config.SshPassword));
            }
            else
            {
                throw new ArgumentException("Either SSH password or key path is required");
            }

            return definition.OperationType?.ToUpper() switch
            {
                "FILE_UPLOAD" => await ExecuteFileUploadAsync(connectionInfo, config, parameters),
                "FILE_DOWNLOAD" => await ExecuteFileDownloadAsync(connectionInfo, config, parameters),
                "WRITE" => await ExecuteCommandAsync(connectionInfo, config, parameters, isWrite: true),
                "READ" or _ => await ExecuteCommandAsync(connectionInfo, config, parameters, isWrite: false)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SSH execution failed");
            return new FunctionExecutionResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                ExecutedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<FunctionExecutionResponse> ExecuteCommandAsync(
        Renci.SshNet.ConnectionInfo connectionInfo,
        ExecutionConfig config,
        Dictionary<string, object>? parameters,
        bool isWrite)
    {
        return await Task.Run(() =>
        {
            using var client = new SshClient(connectionInfo);
            client.Connect();

            // Get command from parameters or config
            var command = parameters?.ContainsKey("command") == true
                ? parameters["command"]?.ToString() ?? ""
                : config.CustomFields?.ContainsKey("command") == true
                    ? config.CustomFields["command"]?.ToString() ?? ""
                    : throw new ArgumentException("Command is required");

            var result = client.RunCommand(command);
            client.Disconnect();

            return new FunctionExecutionResponse
            {
                Success = result.ExitStatus == 0,
                Result = new
                {
                    output = result.Result,
                    error = result.Error,
                    exitStatus = result.ExitStatus
                },
                ExecutedAt = DateTime.UtcNow
            };
        });
    }

    private async Task<FunctionExecutionResponse> ExecuteFileUploadAsync(
        Renci.SshNet.ConnectionInfo connectionInfo,
        ExecutionConfig config,
        Dictionary<string, object>? parameters)
    {
        return await Task.Run(() =>
        {
            var localPath = parameters?.ContainsKey("localPath") == true
                ? parameters["localPath"]?.ToString()
                : config.LocalPath;

            var remotePath = parameters?.ContainsKey("remotePath") == true
                ? parameters["remotePath"]?.ToString()
                : config.RemotePath;

            if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
                throw new ArgumentException("Valid local file path is required");

            if (string.IsNullOrEmpty(remotePath))
                throw new ArgumentException("Remote path is required");

            var fileInfo = new FileInfo(localPath);
            var maxSizeMb = config.MaxFileSizeMb ?? 100;
            if (fileInfo.Length > maxSizeMb * 1024 * 1024)
                throw new InvalidOperationException($"File size exceeds maximum allowed size of {maxSizeMb}MB");

            using var client = new SftpClient(connectionInfo);
            client.Connect();

            using var fileStream = File.OpenRead(localPath);
            client.UploadFile(fileStream, remotePath, canOverride: true);
            
            client.Disconnect();

            return new FunctionExecutionResponse
            {
                Success = true,
                Result = new
                {
                    localPath,
                    remotePath,
                    fileSizeBytes = fileInfo.Length,
                    fileName = fileInfo.Name
                },
                ExecutedAt = DateTime.UtcNow
            };
        });
    }

    private async Task<FunctionExecutionResponse> ExecuteFileDownloadAsync(
        Renci.SshNet.ConnectionInfo connectionInfo,
        ExecutionConfig config,
        Dictionary<string, object>? parameters)
    {
        return await Task.Run(() =>
        {
            var remotePath = parameters?.ContainsKey("remotePath") == true
                ? parameters["remotePath"]?.ToString()
                : config.RemotePath;

            var localPath = parameters?.ContainsKey("localPath") == true
                ? parameters["localPath"]?.ToString()
                : config.LocalPath;

            if (string.IsNullOrEmpty(remotePath))
                throw new ArgumentException("Remote path is required");

            if (string.IsNullOrEmpty(localPath))
                throw new ArgumentException("Local path is required");

            using var client = new SftpClient(connectionInfo);
            client.Connect();

            if (!client.Exists(remotePath))
                throw new FileNotFoundException($"Remote file not found: {remotePath}");

            var fileAttrs = client.GetAttributes(remotePath);
            var maxSizeMb = config.MaxFileSizeMb ?? 100;
            if (fileAttrs.Size > maxSizeMb * 1024 * 1024)
                throw new InvalidOperationException($"File size exceeds maximum allowed size of {maxSizeMb}MB");

            // Ensure local directory exists
            var directory = Path.GetDirectoryName(localPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var fileStream = File.Create(localPath);
            client.DownloadFile(remotePath, fileStream);
            
            client.Disconnect();

            return new FunctionExecutionResponse
            {
                Success = true,
                Result = new
                {
                    remotePath,
                    localPath,
                    fileSizeBytes = fileAttrs.Size,
                    fileName = Path.GetFileName(localPath)
                },
                ExecutedAt = DateTime.UtcNow
            };
        });
    }
}
