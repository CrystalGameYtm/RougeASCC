using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RougeASCC.Models;

namespace RougeASCC.Service; 

public class DataLoaderService
{
    public Dictionary<string, EnemyTemplate> Enemies { get; private set; } = new();
    public Dictionary<string, ItemTemplate> Items { get; private set; } = new();

    public void LoadAllData()
    {
        if (File.Exists("Assets/items.json"))
        {
            string itemsJson = File.ReadAllText("Assets/items.json");
            var itemList = JsonSerializer.Deserialize<List<ItemTemplate>>(itemsJson);
            if (itemList != null)
            {
                foreach (var item in itemList) Items[item.Id] = item;
            }
        }

        if (File.Exists("Assets/enemies.json"))
        {
            string enemiesJson = File.ReadAllText("Assets/enemies.json");
            var enemyList = JsonSerializer.Deserialize<List<EnemyTemplate>>(enemiesJson);
            if (enemyList != null)
            {
                foreach (var enemy in enemyList) Enemies[enemy.Id] = enemy;
            }
        }
    }
}