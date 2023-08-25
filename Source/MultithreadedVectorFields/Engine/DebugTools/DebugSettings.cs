// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MultithreadedVectorFields.Engine.IO;
using Newtonsoft.Json;
using System.IO;

namespace MultithreadedVectorFields.Engine.DebugTools;

public class DebugSettings
{
    [JsonIgnore]
    public static string FullPath => Path.Combine(BaseGame.LocalApplicationDataPath, "DebugSettings.json");

    public static DebugSettings Create()
    {
        DebugSettings settings = null;
        try
        {
            if (File.Exists(FullPath))
            {
                var json = File.ReadAllText(FullPath);
                return JsonConvert.DeserializeObject<DebugSettings>(json);
            }
        }
        catch
        {

        }

        return settings ?? new DebugSettings();

    }

    public bool ShowEntityPositions;
    public bool ShowEntityCollisionBounds;
    public bool ShowEntityCollisionRadius;

    public bool ShowGcCounter;
    public bool ShowFpsCounter;
    public bool ShowTimeRuler;
    public bool ShowPlots;

    public void Save()
    {
        FileManager.EnsureFileDirectory(FullPath);

        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(FullPath, json);
    }
}