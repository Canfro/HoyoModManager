using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoyoModManager.Models;

public class Config
{
    public Dictionary<string, string> Paths { get; set; } = [];
    
    public static Config Current { get; private set; } = new();

    [JsonConstructor]
    private Config()
    {
        Paths.Add("GenshinPath", "");
        Paths.Add("StarRailPath", "");
        Paths.Add("ZenlessPath", "");
    }
        
    public static void Load()
    {
        if (!File.Exists("config.json"))
        {
            string jsonWrite = JsonSerializer.Serialize(Current,  new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("config.json", jsonWrite);
            return;
        }
        
        string jsonRead = File.ReadAllText("config.json");
        Config? loaded = JsonSerializer.Deserialize<Config>(jsonRead);
        
        if (loaded != null)
            Current = loaded;
    }
}