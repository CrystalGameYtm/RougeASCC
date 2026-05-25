using System;
using System.Collections.Generic;
using RougeASCC.Models;
using RougeASCC.Service;
using System.Linq;
using Avalonia.Input;

namespace RougeASCC.ViewModels;

public class GameViewModel 
{
    private readonly AudioService _audio = new();
    public MapModel CurrentMap { get; private set; }
    public PlayerModel Player { get; private set; }
    private readonly HashSet<Key> _pressedKeys = new();
    public GameViewModel(AudioService audio)
    {
        _audio = audio;
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
                    
                    Player.RenderX = x;
                    Player.RenderY = y;
                    return;
                }
            }
        }
    }
    
    public void UpdateEnemiesAI()
    {
        int visionRadius = 8; 
        foreach (var enemy in CurrentMap.Enemies)
        {
            enemy.RenderX += (enemy.LogicX - enemy.RenderX) * 0.2;
            enemy.RenderY += (enemy.LogicY - enemy.RenderY) * 0.2;
            if (enemy.MoveCooldown > 0)
            {
                enemy.MoveCooldown--;
                continue; 
            }
            int distance = Math.Abs(Player.LogicX - enemy.LogicX) + Math.Abs(Player.LogicY - enemy.LogicY);
            if (distance == 1)
            {
                Player.HP -= 3; 
                enemy.MoveCooldown = 45; 
                if (Player.HP < 0) Player.HP = 0;
                return;
            }
            if (distance > 1 && distance <= visionRadius)
            {
                int dx = 0;
                int dy = 0;
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
                if (CurrentMap.Tiles[newX, newY] == TileType.Floor)
                {
                    enemy.LogicX = newX;
                    enemy.LogicY = newY;
                    enemy.MoveCooldown = 30; 
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
        var targetEnemy = CurrentMap.Enemies.FirstOrDefault(e => e.LogicX == newX && e.LogicY == newY);
        if (targetEnemy != null)
        {
            PlayerAttack(targetEnemy);
            return; 
        }
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
        int damage = Player.TotalDamage; 
        enemy.Health -= damage;
        _audio.PlaySoundEffect("hit.mp3"); 
        if (enemy.Health <= 0)
        {
            CurrentMap.Enemies.Remove(enemy);
            _audio.PlaySoundEffect("death_enemy.mp3"); 
        }
    }
    public void UpdateVisuals()
    {
        double speed = 0.3; 
        
        Player.RenderX += (Player.LogicX - Player.RenderX) * speed;
        Player.RenderY += (Player.LogicY - Player.RenderY) * speed;
        if (Math.Abs(Player.RenderX - Player.LogicX) < 0.05 && 
            Math.Abs(Player.RenderY - Player.LogicY) < 0.05)
        {
            Player.IsMoving = false;
        }
    }
 
    private void CheckItemPickup()
    {
        _audio.PlaySoundEffect("pickup.mp3");
        var item = CurrentMap.Items.FirstOrDefault(i => i.X == Player.LogicX && i.Y == Player.LogicY);
        if (item != null)
        {
            
            if (item.Category == ItemCategory.SpaceJump)
            {
                if (!Player.HasSpaceJump) 
                {
                    Player.HasSpaceJump = true;
                    CurrentMap.Items.Remove(item);
                }
            }
            else if (item.Category == ItemCategory.MinorUpgrade)
            {
                Player.Inventory.Add(item);
                CurrentMap.Items.Remove(item);
            }
            else if (item.Category == ItemCategory.Weapon)
            {
                if (Player.EquippedWeapon == null) Player.EquippedWeapon = item;
                else Player.Inventory.Add(item); 
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
            Player.ZVelocity = 0.2; 
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
    
    public GameViewModel(PlayerModel savedPlayer, MapModel savedMap, AudioService audio)
    {
        _audio = audio;
        Player = savedPlayer;
        CurrentMap = savedMap;
    }
}