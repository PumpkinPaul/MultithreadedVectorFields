// Copyright Pumpkin Games Ltd. All Rights Reserved.

using System.IO;

namespace MultithreadedVectorFields.Engine.IO;

public static class FileManager
{
    public static void DeleteDirectory(string path, bool recursive)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive);
    }

    public static void EnsureDirectory(string path, bool deleteIfExists = false)
    {
        if (Directory.Exists(path) && deleteIfExists)
            Directory.Delete(path, true);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public static void EnsureFileDirectory(string path, bool deleteIfExists = false)
    {
        var folderName = Path.GetDirectoryName(path);

        EnsureDirectory(folderName, deleteIfExists);
    }

    public static void MoveFile(string source, string destination)
    {
        if (File.Exists(source) == false)
            return;

        if (File.Exists(destination))
            File.Delete(destination);

        File.Move(source, destination);
    }

    public static void CopyFile(string source, string destination, bool overwrite = true)
    {
        if (File.Exists(source) == false)
            return;

        new FileInfo(source).CopyTo(destination, overwrite);
    }

    public static void DeleteFile(string source)
    {
        if (File.Exists(source))
            File.Delete(source);
    }
}