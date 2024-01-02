using Google.Protobuf;
using Saffron.Runtime;

public static class FileStorageAgent
{
    public async static Task<FileStorageClient> Acquire(string name)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.AcquireFileStorageAsync(new AcquireFileStorageRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Name = name,
        });

        return new FileStorageClient(resp.AgentConfigurationId);
    }
}

public class FileStorageClient
{
    Saffron.Common.Uuid AgentConfigurationId;

    public FileStorageClient(Saffron.Common.Uuid agentConfigurationId)
    {
        this.AgentConfigurationId = agentConfigurationId;
    }

    public async Task<byte[]> SimpleGetFile(string filePath)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.SimpleGetFileAsync(new SimpleGetFileRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            ConfigurationId = AgentConfigurationId,
            Path = filePath,
        });

        return resp.Content.ToByteArray();
    }

    public async Task SimplePutFile(string filePath, byte[] content, bool ensureDirectory = false)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.SimplePutFileAsync(new SimplePutFileRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            ConfigurationId = AgentConfigurationId,
            Path = filePath,
            Data = ByteString.CopyFrom(content),
            EnsureDir = ensureDirectory,
        });
    }

    public async Task DeleteFile(string filePath)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.DeleteFileAsync(new DeleteFileRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            ConfigurationId = AgentConfigurationId,
            Path = filePath,
        });
    }

    public async Task<List<FileInfo>> ListFile(string filePath)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.ListFileAsync(new ListFileRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            ConfigurationId = AgentConfigurationId,
            Path = filePath,
        });

        return resp.Files.Select(f => new FileInfo(f)).ToList();
    }

    public async Task CreateDirAll(string directoryPath)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.CreateDirAllAsync(new CreateDirAllRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            ConfigurationId = AgentConfigurationId,
            Path = directoryPath,
        });
    }
}

public enum FileType
{
    FileTypeFile = 1,
    FileTypeDirectory = 2,
    FileTypeSymbolicLink = 3,
}

public class FileInfo
{
    public string Name { get; set; }
    public FileType Type { get; set; }

    public FileInfo(string name, FileType type)
    {
        this.Name = name;
        this.Type = type;
    }

    public FileInfo(ListFileResponse.Types.FileInfo proto)
    {
        this.Name = proto.Name;
        this.Type = proto.Type switch
        {
            ListFileResponse.Types.FileType.File => FileType.FileTypeFile,
            ListFileResponse.Types.FileType.Directory => FileType.FileTypeDirectory,
            ListFileResponse.Types.FileType.SymbolicLink => FileType.FileTypeSymbolicLink,
            _ => FileType.FileTypeFile,
        };
    }
}
