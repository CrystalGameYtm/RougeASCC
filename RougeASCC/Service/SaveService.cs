using System.IO;
using System.Text.Json;
using RougeASCC.Models;

namespace RougeASCC.Service;

public class GameState
{
    public PlayerModel Player { get; set; }
    public MapModel Map { get; set; }
}

public class SaveService
{
    private const string SavePath = "save.json";

    public void SaveGame(PlayerModel player, MapModel map)
    {
        var state = new GameState { Player = player, Map = map };
        string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SavePath, json);
    }

    public GameState? LoadGame()
    {
        if (!File.Exists(SavePath)) return null;
        
        string json = File.ReadAllText(SavePath);
        return JsonSerializer.Deserialize<GameState>(json);
    }

    public bool HasSave() => File.Exists(SavePath);
}