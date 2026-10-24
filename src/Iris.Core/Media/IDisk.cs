namespace Iris.Core;

/// <summary>The files a repair touches besides the ones the video tools write.</summary>
public interface IDisk
{
    bool Exists(string path);

    void CreateFolder(string path);

    /// <summary>With everything in it; nothing happens when it is not there.</summary>
    void DeleteFolder(string path);

    /// <summary>Fails when <paramref name="to"/> exists.</summary>
    void Move(string from, string to);
}

public sealed class Disk : IDisk
{
    public bool Exists(string path) => File.Exists(path) || Directory.Exists(path);

    public void CreateFolder(string path) => Directory.CreateDirectory(path);

    public void DeleteFolder(string path)
    {
        if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
    }

    public void Move(string from, string to) => File.Move(from, to, overwrite: false);
}
