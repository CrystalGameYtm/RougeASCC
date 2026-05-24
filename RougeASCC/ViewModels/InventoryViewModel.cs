using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RougeASCC.Models;

namespace RougeASCC.ViewModels;

public class InventoryViewModel : INotifyPropertyChanged
{
    public PlayerModel Player { get; }
    public MapModel CurrentMap { get; }
    public ObservableCollection<ItemModel> Items { get; }

    private ItemModel? _selectedItem;
    public ItemModel? SelectedItem 
    { 
        get => _selectedItem;
        set 
        { 
            _selectedItem = value; 
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsUpgradeSelected));
        }
    }

    public bool IsUpgradeSelected => SelectedItem?.Category == ItemCategory.MinorUpgrade;

    public InventoryViewModel(PlayerModel player, MapModel currentMap)
    {
        Player = player;
        CurrentMap = currentMap;
        Items = new ObservableCollection<ItemModel>(player.Inventory);
    }

    // --- МЕТОДИ ДІЙ ---

    public void ApplyToSelf()
    {
        if (SelectedItem != null && Player.PersonalUpgrades.Count < 3)
        {
            Player.PersonalUpgrades.Add(SelectedItem);
            RemoveFromInventory(SelectedItem);
        }
    }

    public void SocketToWeapon()
    {
        if (SelectedItem != null && Player.EquippedWeapon != null && 
            Player.EquippedWeapon.Sockets.Count < Player.EquippedWeapon.MaxSockets)
        {
            Player.EquippedWeapon.Sockets.Add(SelectedItem);
            RemoveFromInventory(SelectedItem);
        }
    }

    public void SocketToArmor()
    {
        if (SelectedItem != null && Player.EquippedArmor != null && 
            Player.EquippedArmor.Sockets.Count < Player.EquippedArmor.MaxSockets)
        {
            Player.EquippedArmor.Sockets.Add(SelectedItem);
            RemoveFromInventory(SelectedItem);
        }
    }

    private void RemoveFromInventory(ItemModel item)
    {
        Player.Inventory.Remove(item);
        Items.Remove(item);
        SelectedItem = null;
        // Оновлюємо статистику (HpStatus і т.д. автоматично перерахуються через властивості PlayerModel)
        OnPropertyChanged(nameof(HpStatus));
    }

    // Властивості для відображення в UI
    public string HpStatus => $"{Player.HP} / {Player.TotalMaxHP}";
    public string DamageStatus => $"{Player.TotalDamage}";
    public string DefenseStatus => $"{Player.TotalDefense}";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}