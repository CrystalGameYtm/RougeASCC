using Avalonia.Controls;
using Avalonia.Interactivity;
using RougeASCC.Service;

namespace RougeASCC.Views;

public partial class MainMenuControl : UserControl
{
    private SaveService _saveService = new();

    public MainMenuControl()
    {
        InitializeComponent();
        ContinueBtn.IsEnabled = _saveService.HasSave();
    }

    private void NewGame_Click(object? sender, RoutedEventArgs e)
    {
        StartGame(null);
    }

    private void Continue_Click(object? sender, RoutedEventArgs e)
    {
        StartGame(_saveService.LoadGame());
    }

    private void Settings_Click(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is MainWindow mainWindow)
        {
            mainWindow.SwitchToSettings();
        }
    }

    private void Exit_Click(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            window.Close();
        }
    }

    private void StartGame(GameState? savedState)
    {
        if (TopLevel.GetTopLevel(this) is MainWindow mainWindow)
        {
            mainWindow.SwitchToGame(savedState);
        }
    }
}