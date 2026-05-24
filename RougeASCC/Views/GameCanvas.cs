using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using RougeASCC.Models;
using RougeASCC.ViewModels;
namespace RougeASCC.Views;

public class GameCanvas : Control
{
    private readonly GameViewModel _viewModel;
    private readonly HashSet<Key> _pressedKeys = new();
    private readonly DispatcherTimer _gameLoopTimer;
    private int _animationTick = 0;
    private int _walkFrameCounter = 0;
    private int _moveCooldown = 0;
    private readonly Typeface _typeface = new(FontFamily.Default, FontStyle.Normal, FontWeight.Bold);
    private readonly int _fontSize = 24;
    private readonly double _cellSize = 18; 
    public GameCanvas(GameViewModel viewModel)
    {
        _viewModel = viewModel;
        Focusable = true;

        _gameLoopTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _gameLoopTimer.Tick += GameLoopTick;
        _gameLoopTimer.Start();
    }

    protected override async void OnKeyDown(KeyEventArgs e)
    {
        int dx = 0, dy = 0;

        if (e.Key == Key.W || e.Key == Key.Up) dy = -1;
        if (e.Key == Key.S || e.Key == Key.Down) dy = 1;
        if (e.Key == Key.A || e.Key == Key.Left) dx = -1;
        if (e.Key == Key.D || e.Key == Key.Right) dx = 1;

        if (dx != 0 || dy != 0) _viewModel.MovePlayer(dx, dy);
        
        if (e.Key == Key.Space) 
        {
            _viewModel.TryJump();
        }
        if (e.Key == Key.E)
        {
            _gameLoopTimer.Stop();
            
            var invViewModel = new InventoryViewModel(_viewModel.Player, _viewModel.CurrentMap); 
            var invWindow = new InventoryWindow { DataContext = invViewModel };
            
            var topLevel = TopLevel.GetTopLevel(this) as Window;
            if (topLevel != null)
            {
                await invWindow.ShowDialog(topLevel);
            }
            
            _gameLoopTimer.Start(); 
        }
        e.Handled = true;
    }
    protected override void OnKeyUp(KeyEventArgs e)
    {
        _pressedKeys.Remove(e.Key);
        e.Handled = true;
    }
    private void GameLoopTick(object? sender, EventArgs e)
    {
        _animationTick++; 
        _viewModel.UpdateEnemiesAI();
        
        bool isMovingInput = _pressedKeys.Contains(Key.W) || _pressedKeys.Contains(Key.S) || 
                             _pressedKeys.Contains(Key.A) || _pressedKeys.Contains(Key.D) ||
                             _pressedKeys.Contains(Key.Up) || _pressedKeys.Contains(Key.Down) || 
                             _pressedKeys.Contains(Key.Left) || _pressedKeys.Contains(Key.Right);
        if (isMovingInput && _viewModel.Player.LogicZ == 0)
        {
            _walkFrameCounter++;
        }
        if (_moveCooldown > 0) 
        {
            _moveCooldown--;
        }
        else
        {
            int dx = 0, dy = 0;
            if (_pressedKeys.Contains(Key.W) || _pressedKeys.Contains(Key.Up)) dy = -1;
            if (_pressedKeys.Contains(Key.S) || _pressedKeys.Contains(Key.Down)) dy = 1;
            if (_pressedKeys.Contains(Key.A) || _pressedKeys.Contains(Key.Left)) dx = -1;
            if (_pressedKeys.Contains(Key.D) || _pressedKeys.Contains(Key.Right)) dx = 1;

            if (dx != 0 || dy != 0)
            {
                _viewModel.MovePlayer(dx, dy);
                _moveCooldown = 10; 
            }
        }

        _viewModel.UpdatePhysics();
        _viewModel.UpdateVisuals(); 
        InvalidateVisual();
    }
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var map = _viewModel.CurrentMap;
        var player = _viewModel.Player;

        double screenCenterX = Bounds.Width / 2;
        double screenCenterY = (Bounds.Height - 40) / 2; 

        double camOffsetX = screenCenterX - (player.RenderX * _cellSize);
        double camOffsetY = screenCenterY - (player.RenderY * _cellSize);

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                double drawX = (x * _cellSize) + camOffsetX;
                double drawY = (y * _cellSize) + camOffsetY;

                if (drawX > -_cellSize && drawX < Bounds.Width && drawY > -_cellSize && drawY < Bounds.Height)
                {
                    bool isWall = map.Tiles[x, y] == TileType.Wall;
                    string symbol = isWall ? "#" : ".";
                    IBrush brush = isWall ? Brushes.SlateGray : Brushes.DarkSlateGray;
                    DrawSymbol(context, symbol, drawX, drawY, brush);
                }
            }
        }

        foreach (var item in map.Items)
        {
            double itemX = (item.X * _cellSize) + camOffsetX;
            double itemY = (item.Y * _cellSize) + camOffsetY;
            
            IBrush itemBrush = Brushes.Gold;
            if (item.Category == ItemCategory.Weapon) itemBrush = (_animationTick % 60 < 30) ? Brushes.Red : Brushes.DarkRed;
            else if (item.Category == ItemCategory.SpaceJump) itemBrush = (_animationTick % 30 < 15) ? Brushes.Magenta : Brushes.DeepPink;
            else if (item.Category == ItemCategory.MinorUpgrade) itemBrush = Brushes.Orange;
            DrawSymbol(context, item.Symbol.ToString(), itemX, itemY, itemBrush);
        }

        foreach (var enemy in map.Enemies)
        {
            double enemyX = (enemy.RenderX * _cellSize) + camOffsetX;
            double enemyY = (enemy.RenderY * _cellSize) + camOffsetY;
            
            DrawSymbol(context, enemy.Symbol.ToString(), enemyX, enemyY, Brushes.LimeGreen);
        }

        double px = screenCenterX; 
        double py = screenCenterY; 
        
        IBrush shadowBrush = new SolidColorBrush(Color.FromArgb(180, 150, 150, 150));
        DrawSymbol(context, ".", px, py + 10, shadowBrush); 

        double renderY = py - (player.LogicZ * 40); 
        
        string playerSymbol = "@";

        // 1. ПРІОРИТЕТ 1: Анімація стрибка (в повітрі)
        if (player.LogicZ > 0)
        {
            string[] jumpAnim = { "*", "X", "+", "x" }; 
            playerSymbol = jumpAnim[(_animationTick / 3) % jumpAnim.Length];
        }
        else
        {
            bool isMovingInput = _pressedKeys.Contains(Key.W) || _pressedKeys.Contains(Key.S) || 
                                 _pressedKeys.Contains(Key.A) || _pressedKeys.Contains(Key.D) ||
                                 _pressedKeys.Contains(Key.Up) || _pressedKeys.Contains(Key.Down) || 
                                 _pressedKeys.Contains(Key.Left) || _pressedKeys.Contains(Key.Right);
            
            bool isSliding = Math.Abs(player.RenderX - player.LogicX) > 0.05 || 
                             Math.Abs(player.RenderY - player.LogicY) > 0.05;

            if (isMovingInput || isSliding)
            {
                string[] walkLeftRight = { "(", "|", ")" }; 
                string[] walkUpDown = { "v", "-", "^" };
                
                // Змінюємо кадр кожні 4 тіки використовуючи наш новий лічильник
                int frame = (_walkFrameCounter / 4) % 3;
                
                // Визначаємо, який набір символів показати
                if (Math.Abs(player.FacingX) > 0) // Якщо останній рух був по горизонталі
                {
                     playerSymbol = walkLeftRight[frame];
                }
                else // Якщо по вертикалі
                {
                     playerSymbol = walkUpDown[frame];
                }
            }
            else
            {
                // 3. ПРІОРИТЕТ 3: Стан спокою (Idle)
                if (player.FacingX > 0) playerSymbol = ">";
                else if (player.FacingX < 0) playerSymbol = "<";
                else if (player.FacingY > 0) playerSymbol = "v";
                else if (player.FacingY < 0) playerSymbol = "^";
                else playerSymbol = "@"; 
            }
        }
        IBrush playerBrush = player.HasSpaceJump ? Brushes.Cyan : Brushes.White; 
        DrawSymbol(context, ".", player.RenderX * _cellSize, player.RenderY * _cellSize, shadowBrush);
        DrawSymbol(context, playerSymbol, px, renderY, playerBrush);

        DrawHUD(context, player);
    }
    
    private void DrawHUD(DrawingContext context, PlayerModel player)
    {
        double hudHeight = 40;
        var hudRect = new Rect(0, Bounds.Height - hudHeight, Bounds.Width, hudHeight);
        
        context.DrawRectangle(Brushes.Black, null, hudRect);
        context.DrawLine(new Pen(Brushes.White, 2), new Point(0, Bounds.Height - hudHeight), new Point(Bounds.Width, Bounds.Height - hudHeight));

        string weaponName = player.EquippedWeapon != null ? player.EquippedWeapon.Name : "Кулаки";
        string armorName = player.EquippedArmor != null ? player.EquippedArmor.Name : "Одяг";
        
        // Формуємо рядок з нашими новими характеристиками
        string hudText = $"ХП: {player.HP}/{player.TotalMaxHP}  |  УРОН: {player.TotalDamage}  |  ЗАХИСТ: {player.TotalDefense}  |  Зброя: {weaponName}  |  Броня: {armorName}";

        var formattedText = new FormattedText(
            hudText, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold), 14, Brushes.Yellow);
            
        context.DrawText(formattedText, new Point(20, Bounds.Height - hudHeight + 12));
    }

    private void DrawSymbol(DrawingContext context, string text, double x, double y, IBrush brush)
    {
        var formattedText = new FormattedText(
            text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            _typeface, _fontSize, brush);
        context.DrawText(formattedText, new Point(x, y));
    }
}