using System.ComponentModel;
using System.Runtime.CompilerServices;
using RougeASCC.Service;

namespace RougeASCC.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly AudioService _audio;

    public SettingsViewModel(AudioService audio)
    {
        _audio = audio;
    }
    public double MusicVolume
    {
        get => _audio.MusicVolume * 100;
        set
        {
            _audio.MusicVolume = (float)(value / 100.0);
            OnPropertyChanged();
        }
    }

    public double SfxVolume
    {
        get => _audio.SfxVolume * 100;
        set
        {
            _audio.SfxVolume = (float)(value / 100.0);
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}