using System.Collections.Generic;
using System.Linq;

namespace RougeASCC.Models;

public class PlayerModel
{
    // --- ФІЗИКА ТА КООРДИНАТИ ---
    public int LogicX { get; set; }
    public int LogicY { get; set; }
    public double RenderX { get; set; }
    public double RenderY { get; set; }
    public double LogicZ { get; set; } 
    public double ZVelocity { get; set; }
    
    public bool IsMoving { get; set; } 
    public int FacingX { get; set; } = 0; 
    public int FacingY { get; set; } = 0;

    // --- ІНВЕНТАР ТА ЛІМІТИ ---
    public bool HasSpaceJump { get; set; } = false;
    public List<ItemModel> Inventory { get; set; } = new(); 
    public ItemModel? EquippedWeapon { get; set; } 
    public ItemModel? EquippedArmor { get; set; }
    public List<ItemModel> PersonalUpgrades { get; set; } = new(3); 

    // --- БАЗОВІ ХАРАКТЕРИСТИКИ ---
    public int HP { get; set; } = 100;
    public int BaseMaxHP { get; set; } = 100;
    public int BaseDamage { get; set; } = 5; 
    public int BaseDefense { get; set; } = 0;
    
    // НОВЕ: Базові швидкості. 
    // Наприклад, 10 - це стандартна затримка (MoveCooldown), чим менше значення, тим швидше.
    public int BaseWalkSpeed { get; set; } = 5; 
    public int BaseAttackSpeed { get; set; } = 45; 
    public int BaseWalkCooldown { get; set; } = 20;
    // --- УНІВЕРСАЛЬНИЙ МЕТОД ПОШУКУ ЕФЕКТІВ ---
    // Цей метод шукає вказаний ефект ОДНОЧАСНО і в зброї, і в броні
    private int GetSocketEffectTotal(UpgradeEffect effect)
    {
        int weaponBonus = EquippedWeapon?.Sockets.Where(s => s.Effect == effect).Sum(s => s.EffectValue) ?? 0;
        int armorBonus = EquippedArmor?.Sockets.Where(s => s.Effect == effect).Sum(s => s.EffectValue) ?? 0;
        return weaponBonus + armorBonus;
    }

    // --- ДИНАМІЧНІ ХАРАКТЕРИСТИКИ (Тотал) ---
    
    public int TotalMaxHP => BaseMaxHP 
                             + PersonalUpgrades.Where(u => u.Effect == UpgradeEffect.MaxHP).Sum(u => u.EffectValue)
                             + GetSocketEffectTotal(UpgradeEffect.MaxHP);

    public int TotalDamage => BaseDamage 
                              + (EquippedWeapon?.BasePower ?? 0) // Беремо ТІЛЬКИ чистий урон самої зброї
                              + PersonalUpgrades.Where(u => u.Effect == UpgradeEffect.Damage).Sum(u => u.EffectValue)
                              + GetSocketEffectTotal(UpgradeEffect.Damage); // І додаємо всі руни на урон (звідусіль)

    public int TotalDefense => BaseDefense 
                               + (EquippedArmor?.BasePower ?? 0) // Беремо ТІЛЬКИ чистий захист броні
                               + PersonalUpgrades.Where(u => u.Effect == UpgradeEffect.Defense).Sum(u => u.EffectValue)
                               + GetSocketEffectTotal(UpgradeEffect.Defense);

    // Розрахунок швидкості ходьби (чим вищий бонус, тим менша затримка)
    public int TotalWalkSpeed => BaseWalkSpeed 
                                 - PersonalUpgrades.Where(u => u.Effect == UpgradeEffect.WalkSpeed).Sum(u => u.EffectValue)
                                 - GetSocketEffectTotal(UpgradeEffect.WalkSpeed);

    // Розрахунок швидкості атаки (чим вищий бонус, тим менший відкат атаки)
    public int TotalAttackSpeed => BaseAttackSpeed 
                                   - PersonalUpgrades.Where(u => u.Effect == UpgradeEffect.AttackSpeed).Sum(u => u.EffectValue)
                                   - GetSocketEffectTotal(UpgradeEffect.AttackSpeed);
    public int TotalWalkCooldown => BaseWalkCooldown 
                                    - PersonalUpgrades.Where(u => u.Effect == UpgradeEffect.WalkSpeed).Sum(u => u.EffectValue)
                                    - (EquippedArmor?.Sockets.Where(s => s.Effect == UpgradeEffect.WalkSpeed).Sum(s => s.EffectValue) ?? 0);
}