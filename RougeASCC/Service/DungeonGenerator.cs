using System;
using System.Collections.Generic;
using RougeASCC.Models;

namespace RougeASCC.Service;

public class DungeonGenerator
{
    private Random _rand = new Random();

    public MapModel GenerateRandomMap()
    {
        // Випадкові розміри карти
        int width = _rand.Next(40, 120);
        int height = _rand.Next(30, 180);
        
        // Кількість кімнат залежить від розміру карти
        int roomCount = (width * height) / 400; 
        
        var map = new MapModel(width, height);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map.Tiles[x, y] = TileType.Wall;

        var roomCenters = new List<(int x, int y)>();

        for (int i = 0; i < roomCount; i++)
        {
            int roomWidth = _rand.Next(5, 12);
            int roomHeight = _rand.Next(5, 12);
            int roomX = _rand.Next(1, width - roomWidth - 1);
            int roomY = _rand.Next(1, height - roomHeight - 1);

            for (int x = roomX; x < roomX + roomWidth; x++)
                for (int y = roomY; y < roomY + roomHeight; y++)
                    map.Tiles[x, y] = TileType.Floor;

            roomCenters.Add((roomX + roomWidth / 2, roomY + roomHeight / 2));
            
            // Спавн ворогів у кожній кімнаті (окрім першої, де з'явиться гравець)
            if (i > 0 && _rand.NextDouble() > 0.3) // 70% шанс на ворога в кімнаті
            {
                map.Enemies.Add(new EnemyModel 
                { 
                    LogicX = roomX + 2, 
                    LogicY = roomY + 2,
                    RenderX = roomX + 2,
                    RenderY = roomY + 2,
                    Health = 20, // Згодом це братиметься з JSON
                    Symbol = 'g' // Гоблін
                });
            }
        }

        for (int i = 0; i < roomCenters.Count - 1; i++)
        {
            var current = roomCenters[i];
            var next = roomCenters[i + 1];

            for (int x = Math.Min(current.x, next.x); x <= Math.Max(current.x, next.x); x++)
                map.Tiles[x, current.y] = TileType.Floor;
            for (int y = Math.Min(current.y, next.y); y <= Math.Max(current.y, next.y); y++)
                map.Tiles[next.x, y] = TileType.Floor;
        }

        foreach (var roomCenter in roomCenters)
        {
            // 60% шанс, що в цій кімнаті буде якийсь предмет
            if (_rand.NextDouble() < 0.6)
            {
                // Вибираємо випадкове зміщення від центру кімнати, щоб предмети не лежали в одній купі
                int itemX = roomCenter.x + _rand.Next(-2, 3);
                int itemY = roomCenter.y + _rand.Next(-2, 3);

                // Перевіряємо, чи ми не засунули предмет у стіну
                if (map.Tiles[itemX, itemY] == TileType.Floor)
                {
                    // Визначаємо, ЯКИЙ саме предмет заспавнити
                   double roll = _rand.NextDouble();
                    
                    if (roll < 0.3) // 30% зброя
                    {
                        map.Items.Add(new ItemModel { X = itemX, Y = itemY, Symbol = 'W', Category = ItemCategory.Weapon, Name = "Іржавий Меч", BasePower = 10, MaxSockets = 1 });
                    }
                    else if (roll < 0.6) // 30% броня
                    {
                        map.Items.Add(new ItemModel { X = itemX, Y = itemY, Symbol = 'A', Category = ItemCategory.Armor, Name = "Шкіряна Куртка", BasePower = 3, MaxSockets = 2 });
                    }
                    else if (roll < 0.95) // 35% МІНОРНИЙ АПГРЕЙД
                    {
                        var minorUpgrade = new ItemModel { X = itemX, Y = itemY, Symbol = 'M', Category = ItemCategory.MinorUpgrade };
                        
                        // Рандомно вибираємо, яку саме характеристику покращить цей предмет
                        int effectRoll = _rand.Next(0, 5);
                        
                        switch (effectRoll)
                        {
                            case 0: // Здоров'я
                                minorUpgrade.Effect = UpgradeEffect.MaxHP;
                                minorUpgrade.EffectValue = _rand.Next(10, 26); // +10-25 ХП
                                minorUpgrade.Name = $"Сфера Життя (+{minorUpgrade.EffectValue} ХП)";
                                break;
                            case 1: // Урон
                                minorUpgrade.Effect = UpgradeEffect.Damage;
                                minorUpgrade.EffectValue = _rand.Next(2, 6); // +2-5 Урону
                                minorUpgrade.Name = $"Руна Гніву (+{minorUpgrade.EffectValue} Урон)";
                                break;
                            case 2: // Захист
                                minorUpgrade.Effect = UpgradeEffect.Defense;
                                minorUpgrade.EffectValue = _rand.Next(1, 4); // +1-3 Захисту
                                minorUpgrade.Name = $"Уламок Сталі (+{minorUpgrade.EffectValue} Захист)";
                                break;
                            case 3: // Швидкість ходьби
                                minorUpgrade.Effect = UpgradeEffect.WalkSpeed;
                                minorUpgrade.EffectValue = _rand.Next(1, 3); // Мінус 1-2 кадри затримки
                                minorUpgrade.Name = $"Легка Тканина (+Швидкість руху)";
                                break;
                            case 4: // Швидкість атаки
                                minorUpgrade.Effect = UpgradeEffect.AttackSpeed;
                                minorUpgrade.EffectValue = _rand.Next(2, 6); // Мінус 2-5 кадрів затримки
                                minorUpgrade.Name = $"Амулет Вітру (+Швидкість атаки)";
                                break;
                        }
                        
                        map.Items.Add(minorUpgrade);
                    }
                    else // 5% Спейс Джамп (рідкісний)
                    {
                        map.Items.Add(new ItemModel { X = itemX, Y = itemY, Symbol = 'S', Category = ItemCategory.SpaceJump, Name = "Space Jump Module" });
                    }
                }
            }
        }

        return map;
    }
}