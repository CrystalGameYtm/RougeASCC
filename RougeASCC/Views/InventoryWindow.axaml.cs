using Avalonia.Controls;
using Avalonia.Interactivity;
using RougeASCC.Service;
using RougeASCC.ViewModels;

namespace RougeASCC.Views;

public partial class InventoryWindow : Window
{
    public InventoryWindow()
    {
        InitializeComponent();
    }
    private void ApplyToSelf_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is InventoryViewModel vm) vm.ApplyToSelf();
    }
    private void SaveGame_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is InventoryViewModel vm)
        {
            var saveService = new SaveService();
            saveService.SaveGame(vm.Player, vm.CurrentMap); 
        }
    }
    private void SocketToWeapon_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is InventoryViewModel vm) vm.SocketToWeapon();
    }

    private void SocketToArmor_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is InventoryViewModel vm) vm.SocketToArmor();
    }
    // Той самий метод, який шукає компілятор (Помилка AVLN3000)
    private void CloseWindow_Click(object? sender, RoutedEventArgs e)
    {
        // Вбудований метод Avalonia для закриття поточного вікна
        Close(); 
    }
}