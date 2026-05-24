using Avalonia.Controls;
using RougeASCC.Service;
using RougeASCC.ViewModels;

namespace RougeASCC.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // При запуску гри показуємо Головне Меню
        Content = new MainMenuControl();
    }

    public void SwitchToGame(GameState? savedState)
    {
        GameViewModel viewModel;
        
        if (savedState != null)
        {
            viewModel = new GameViewModel(savedState.Player, savedState.Map);
        }
        else
        {
            viewModel = new GameViewModel();
        }

        Content = new GameCanvas(viewModel);
    }

    public void SwitchToMainMenu()
    {
        Content = new MainMenuControl();
    }

    public void SwitchToSettings()
    {
        Content = new SettingsControl();
    }
}