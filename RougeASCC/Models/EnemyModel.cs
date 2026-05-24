namespace RougeASCC.Models;

public class EnemyModel
{
    public int LogicX { get; set; }
    public int LogicY { get; set; }
    public double RenderX { get; set; }
    public double RenderY { get; set; }
    public int Health { get; set; }
    public char Symbol { get; set; }
    public int MoveCooldown { get; set; } = 0; // Для уповільнення руху ворога
}

public class EnemyTemplate
{
    public string Id { get; set; } = "";
    public char Symbol { get; set; }
    public string Name { get; set; } = "";
    public int Health { get; set; }
    public int Damage { get; set; }
    public string ColorHex { get; set; } = "#FFFFFF"; // Колір для Avalonia
}