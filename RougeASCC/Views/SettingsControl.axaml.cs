using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RougeASCC.Views;

public partial class SettingsControl : UserControl
{
    public SettingsControl()
    {
        InitializeComponent();
    }

    private void Back_Click(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is MainWindow mainWindow)
        {
            mainWindow.SwitchToMainMenu();
        }
    }
}