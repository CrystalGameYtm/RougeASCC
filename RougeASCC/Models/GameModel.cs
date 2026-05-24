using System.Collections.Generic;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace RougeASCC.Models;
public enum TileType { Wall, Floor }
public enum ItemCategory { Weapon, Armor, MinorUpgrade, SpaceJump }
public enum UpgradeEffect { None, MaxHP, Defense, WalkSpeed, Damage, AttackSpeed }

public class MapModel
{
    // Додано set; для того, щоб Json міг записувати сюди дані при завантаженні
    public int Width { get; set; }
    public int Height { get; set; }

    // Кажемо Json повністю ігнорувати цю змінну під час збереження
    [JsonIgnore] 
    public TileType[,] Tiles { get; set; }

    // Спеціальний транслятор для збереження карти
    public TileType[][] SavedTiles
    {
        get
        {
            if (Tiles == null) return null!;
            var result = new TileType[Width][];
            for (int x = 0; x < Width; x++)
            {
                result[x] = new TileType[Height];
                for (int y = 0; y < Height; y++)
                {
                    result[x][y] = Tiles[x, y];
                }
            }
            return result;
        }
        set
        {
            if (value == null) return;
            Width = value.Length;
            Height = value.Length > 0 ? value[0].Length : 0;
            Tiles = new TileType[Width, Height];
            
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Tiles[x, y] = value[x][y];
                }
            }
        }
    }

    public List<ItemModel> Items { get; set; } = new();
    public List<EnemyModel> Enemies { get; set; } = new();

    // Обов'язковий порожній конструктор для десеріалізації Json
    public MapModel() { }

    public MapModel(int width, int height)
    {
        Width = width;
        Height = height;
        Tiles = new TileType[width, height];
    }
}


public class ItemModel
{
    public int X { get; set; }
    public int Y { get; set; }
    public char Symbol { get; set; }
    public ItemCategory Category { get; set; }
    public string Name { get; set; } = "Предмет";
    
    // Базовий показник (урон для зброї, захист для броні)
    public int BasePower { get; set; } 

    // -- ДЛЯ МІНОРНИХ АПГРЕЙДІВ --
    public UpgradeEffect Effect { get; set; } = UpgradeEffect.None;
    public int EffectValue { get; set; }

    // -- ДЛЯ ЗБРОЇ ТА БРОНІ (Система слотів) --
    public int MaxSockets { get; set; } = 2; // n кількість слотів
    public List<ItemModel> Sockets { get; set; } = new(); // Вставлені мінорні апгрейди

    // Магія MVVM: Динамічний розрахунок загальної сили предмета з урахуванням вставлених апгрейдів
    public int TotalPower => BasePower + Sockets
        .Where(s => s.Effect == UpgradeEffect.Damage || s.Effect == UpgradeEffect.Defense)
        .Sum(s => s.EffectValue);
}
public class ItemTemplate
{
    public string Id { get; set; } = "";
    public char Symbol { get; set; }
    public int Category { get; set; }
    public string Name { get; set; } = "";
    public int Power { get; set; } 
}
