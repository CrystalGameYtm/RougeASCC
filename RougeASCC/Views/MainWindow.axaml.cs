using Avalonia.Controls;
using RougeASCC.Service;
using RougeASCC.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace RougeASCC.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Content = new MainMenuControl();
    }

    public void SwitchToGame(GameState? savedState)
    {
        // ПРОСИМО СЕРВІСИ З DI КОНТЕЙНЕРА
        var audio = App.Services!.GetRequiredService<AudioService>();
        
        audio.StopMusic();
        audio.PlayMusic("bgm.mp3");

        GameViewModel viewModel;
        if (savedState != null)
        {
            viewModel = new GameViewModel(audio, savedState.Player, savedState.Map);
        }
        else
        {
            // Беремо новий екземпляр гри з усіма підключеними залежностями
            viewModel = App.Services!.GetRequiredService<GameViewModel>();
        }

        Content = new GameCanvas(viewModel);
    }

    public void SwitchToMainMenu()
    {
        var audio = App.Services!.GetRequiredService<AudioService>();
        audio.StopMusic();
        Content = new MainMenuControl();
    }

    public void SwitchToSettings()
    {
        Content = new SettingsControl();
    }
}