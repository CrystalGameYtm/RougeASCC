using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RougeASCC.Service;
using RougeASCC.ViewModels;
using RougeASCC.Views;
using System;

namespace RougeASCC;

public partial class App : Application
{
    // Глобальний доступ до наших сервісів
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 1. СТВОРЕННЯ КОНТЕЙНЕРА DI
        var services = new ServiceCollection();

        // 2. РЕЄСТРАЦІЯ СЕРВІСІВ (Singleton - один екземпляр на всю гру)
        services.AddSingleton<AudioService>();
        services.AddSingleton<SaveService>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<GameViewModel>();
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}