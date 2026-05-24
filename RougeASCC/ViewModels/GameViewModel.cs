using System;
using System.Collections.Generic;
using RougeASCC.Models;
using RougeASCC.Service;
using System.Linq;
using Avalonia.Input;

namespace RougeASCC.ViewModels;


public class GameViewModel 
{
    public MapModel CurrentMap { get; private set; }
    public PlayerModel Player { get; private set; }
    private readonly HashSet<Key> _pressedKeys = new();
    public GameViewModel()
    {
        var generator = new DungeonGenerator();
        CurrentMap = generator.GenerateRandomMap();
        Player = new PlayerModel();
        PlacePlayer();
    }

    private void PlacePlayer()
    {
        for (int x = 0; x < CurrentMap.Width; x++)
        {
            for (int y = 0; y < CurrentMap.Height; y++)
            {
                if (CurrentMap.Tiles[x, y] == TileType.Floor)
                {
                    Player.LogicX = x;
                    Player.LogicY = y;
                    
                    // ВАЖЛИВО: На старті візуальна позиція збігається з логічною
                    Player.RenderX = x;
                    Player.RenderY = y;
                    return;
                }
            }
        }
    }
    
    public void UpdateEnemiesAI()
    {
        int visionRadius = 8; // Скільки клітинок бачить ворог

        foreach (var enemy in CurrentMap.Enemies)
        {
            // Плавно підтягуємо візуал ворога до його логіки (як у гравця)
            enemy.RenderX += (enemy.LogicX - enemy.RenderX) * 0.2;
            enemy.RenderY += (enemy.LogicY - enemy.RenderY) * 0.2;

            // Кульдаун, щоб ворог ходив повільніше за 60 FPS
            if (enemy.MoveCooldown > 0)
            {
                enemy.MoveCooldown--;
                continue; 
            }

            // Розрахунок дистанції (Манхеттенська відстань)
            int distance = Math.Abs(Player.LogicX - enemy.LogicX) + Math.Abs(Player.LogicY - enemy.LogicY);
            if (distance == 1)
            {
                // Атака на гравця
                Player.HP -= 3; // Ворог завдає шкоди
                enemy.MoveCooldown = 45; // Збільшений відкат після атаки
        
                if (Player.HP < 0) Player.HP = 0; // Смерть гравця (Game Over логіка)
                return;
            }
            // Якщо гравець у зоні видимості - переслідуємо
            if (distance > 1 && distance <= visionRadius)
            {
                int dx = 0;
                int dy = 0;

                // Визначаємо, по якій осі відстань більша, і йдемо туди
                if (Math.Abs(Player.LogicX - enemy.LogicX) > Math.Abs(Player.LogicY - enemy.LogicY))
                {
                    dx = Player.LogicX > enemy.LogicX ? 1 : -1;
                }
                else
                {
                    dy = Player.LogicY > enemy.LogicY ? 1 : -1;
                }

                int newX = enemy.LogicX + dx;
                int newY = enemy.LogicY + dy;

                // Перевірка, чи не врізається ворог у стіну
                if (CurrentMap.Tiles[newX, newY] == TileType.Floor)
                {
                    enemy.LogicX = newX;
                    enemy.LogicY = newY;
                    enemy.MoveCooldown = 30; // Чекаємо 30 кадрів (півсекунди) до наступного кроку
                }
            }
        }
    }
    
    public void MovePlayer(int dx, int dy)
    {
        Player.FacingX = dx;
        Player.FacingY = dy;

        int newX = Player.LogicX + dx;
        int newY = Player.LogicY + dy;

        // ПЕРЕВІРКА: Чи є ворог на цій клітинці?
        var targetEnemy = CurrentMap.Enemies.FirstOrDefault(e => e.LogicX == newX && e.LogicY == newY);
        if (targetEnemy != null)
        {
            // Якщо ворог є - атакуємо його замість ходьби
            PlayerAttack(targetEnemy);
            return; // Перериваємо рух
        }

        // Якщо ворога немає - звичайний рух
        if (CurrentMap.Tiles[(int)newX, (int)newY] == TileType.Floor)
        {
            Player.LogicX = newX;
            Player.LogicY = newY;
            Player.IsMoving = true; 
            CheckItemPickup();
        }
    }

    private void PlayerAttack(EnemyModel enemy)
    {
        // Використовуємо наш новий динамічний прорахунок загального урона!
        int damage = Player.TotalDamage; 
        
        enemy.Health -= damage;

        if (enemy.Health <= 0)
        {
            CurrentMap.Enemies.Remove(enemy);
        }
    }
    
    
    public void UpdateVisuals()
    {
        double speed = 0.3; 
        
        Player.RenderX += (Player.LogicX - Player.RenderX) * speed;
        Player.RenderY += (Player.LogicY - Player.RenderY) * speed;

        // Якщо різниця між логікою та візуалом дуже мала - ми зупинилися
        if (Math.Abs(Player.RenderX - Player.LogicX) < 0.05 && 
            Math.Abs(Player.RenderY - Player.LogicY) < 0.05)
        {
            Player.IsMoving = false;
        }
    }
 
    private void CheckItemPickup()
    {
        var item = CurrentMap.Items.FirstOrDefault(i => i.X == Player.LogicX && i.Y == Player.LogicY);
        if (item != null)
        {
            if (item.Category == ItemCategory.SpaceJump)
            {
                // ЛІМІТ: Мажорний апгрейд підбирається лише раз
                if (!Player.HasSpaceJump) 
                {
                    Player.HasSpaceJump = true;
                    CurrentMap.Items.Remove(item);
                }
            }
            else if (item.Category == ItemCategory.MinorUpgrade)
            {
                // Мінорні апгрейди тепер йдуть у загальний інвентар.
                // Гравець сам вирішуватиме в меню паузи, куди їх вставити: у тіло (до 3), у зброю чи броню
                Player.Inventory.Add(item);
                CurrentMap.Items.Remove(item);
            }
            else if (item.Category == ItemCategory.Weapon)
            {
                if (Player.EquippedWeapon == null) Player.EquippedWeapon = item;
                else Player.Inventory.Add(item); // Якщо вже є зброя, кидаємо нову в рюкзак
                CurrentMap.Items.Remove(item);
            }
            else if (item.Category == ItemCategory.Armor)
            {
                if (Player.EquippedArmor == null) Player.EquippedArmor = item;
                else Player.Inventory.Add(item);
                CurrentMap.Items.Remove(item);
            }
        }
    }

    public void TryJump()
    {
        if (Player.LogicZ == 0 || Player.HasSpaceJump)
        {
            Player.ZVelocity = 0.2; // Імпульс стрибка
        }
    }

    public void UpdatePhysics()
    {
        if (Player.LogicZ > 0 || Player.ZVelocity > 0)
        {
            Player.ZVelocity -= 0.03;
            Player.LogicZ += Player.ZVelocity;

            if (Player.LogicZ <= 0)
            {
                Player.LogicZ = 0;
                Player.ZVelocity = 0;
            }
        }
    }
    
    // Конструктор для завантаження збереження
    public GameViewModel(PlayerModel savedPlayer, MapModel savedMap)
    {
        Player = savedPlayer;
        CurrentMap = savedMap;
    }
}