using NetCoreAudio;
using System;
using System.IO;
using System.Collections.Generic;

namespace RougeASCC.Service;

public class AudioService
{
    private readonly Player _musicPlayer = new();
    private readonly List<Player> _sfxPlayers = new();
    private readonly string _logFile = "audio_log.txt";

    public AudioService()
    {
        File.AppendAllText(_logFile, $"\n[{DateTime.Now}] AudioService створено через DI.\n");
    }

    private void Log(string message)
    {
        File.AppendAllText(_logFile, $"[{DateTime.Now}] {message}\n");
    }

    public void PlayMusic(string fileName)
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(baseDir, "Assets", "Music", fileName);

            if (File.Exists(path))
            {
                Log($"Спроба відтворити фонову музику: {path}");
                _musicPlayer.Play(path);
            }
            else
            {
                Log($"ПОМИЛКА: Файл музики не знайдено за шляхом {path}");
            }
        }
        catch (Exception ex)
        {
            Log($"КРИТИЧНА ПОМИЛКА МУЗИКИ: {ex.Message}");
        }
    }

    public void StopMusic() => _musicPlayer.Stop();

    public void PlaySoundEffect(string fileName)
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(baseDir, "Assets", "Sounds", fileName);

            if (File.Exists(path))
            {
                var sfxPlayer = new Player();
                _sfxPlayers.Add(sfxPlayer);
                sfxPlayer.Play(path);

                if (_sfxPlayers.Count > 15)
                {
                    _sfxPlayers[0].Stop();
                    _sfxPlayers.RemoveAt(0);
                }
            }
            else
            {
                Log($"ПОМИЛКА: SFX файл не знайдено: {path}");
            }
        }
        catch (Exception ex)
        {
            Log($"КРИТИЧНА ПОМИЛКА SFX: {ex.Message}");
        }
    }
}