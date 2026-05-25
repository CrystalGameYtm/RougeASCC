using System;
using System.IO;
using System.Collections.Generic;
using Silk.NET.OpenAL;

namespace RougeASCC.Service;

// Додаємо unsafe до всього класу, оскільки ми працюємо з вказівниками пристроїв
public unsafe class AudioService : IDisposable
{
    private readonly AL _al;
    private readonly ALContext _alc; // ЗМІНЕНО: тепер це ALContext

    private Device* _device;
    private Context* _context;

    private readonly Dictionary<string, uint> _buffers = new();
    private uint _musicSource;
    private float _musicVolume = 0.15f; 
    private float _sfxVolume = 0.85f;

    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = value;
            // Якщо музика зараз грає, миттєво оновлюємо її гучність
            if (_musicSource != 0)
            {
                _al.SetSourceProperty(_musicSource, SourceFloat.Gain, _musicVolume);
            }
        }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set => _sfxVolume = value;
    }
    public AudioService()
    {
        _al = AL.GetApi();
        _alc = ALContext.GetApi(); // ЗМІНЕНО

        // Відкриваємо стандартний аудіопристрій
        _device = _alc.OpenDevice("");
        if (_device == null) throw new Exception("Не вдалося відкрити аудіопристрій OpenAL.");

        // Створюємо та активуємо контекст
        _context = _alc.CreateContext(_device, null);
        _alc.MakeContextCurrent(_context);

        _al.SetListenerProperty(ListenerVector3.Position, 0, 0, 0);
    }

    public void PlayMusic(string fileName)
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Music", fileName);
        if (!File.Exists(path)) return;

        if (_musicSource != 0)
        {
            _al.SourceStop(_musicSource);
            _al.DeleteSource(_musicSource);
        }

        uint buffer = GetOrCreateBuffer(path);
        _musicSource = _al.GenSource();

        _al.SetSourceProperty(_musicSource, SourceInteger.Buffer, buffer);
        _al.SetSourceProperty(_musicSource, SourceBoolean.Looping, true);
        
        // Музика тиха (15% від максимуму)
        _al.SetSourceProperty(_musicSource, SourceFloat.Gain, 0.15f);

        _al.SourcePlay(_musicSource);
    }

    public void StopMusic()
    {
        if (_musicSource != 0) _al.SourceStop(_musicSource);
    }

    public void PlaySoundEffect(string fileName)
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", fileName);
        if (!File.Exists(path)) return;

        uint buffer = GetOrCreateBuffer(path);
        
        uint source = _al.GenSource();
        _al.SetSourceProperty(source, SourceInteger.Buffer, buffer);
        
        // Ефекти гучні (85% від максимуму)
        _al.SetSourceProperty(source, SourceFloat.Gain, 0.85f);
        
        _al.SourcePlay(source);
    }

    private uint GetOrCreateBuffer(string filePath)
    {
        if (_buffers.TryGetValue(filePath, out uint existingBuffer))
        {
            return existingBuffer;
        }

        uint buffer = _al.GenBuffer();
        LoadWavFile(filePath, out byte[] data, out int channels, out int sampleRate, out int bitsPerSample);

        BufferFormat format = BufferFormat.Mono8;
        if (channels == 1 && bitsPerSample == 8) format = BufferFormat.Mono8;
        else if (channels == 1 && bitsPerSample == 16) format = BufferFormat.Mono16;
        else if (channels == 2 && bitsPerSample == 8) format = BufferFormat.Stereo8;
        else if (channels == 2 && bitsPerSample == 16) format = BufferFormat.Stereo16;

        // Пряма робота з пам'яттю
        fixed (byte* pData = data)
        {
            _al.BufferData(buffer, format, pData, data.Length, sampleRate);
        }

        _buffers[filePath] = buffer;
        return buffer;
    }

    private static void LoadWavFile(string filename, out byte[] audioData, out int channels, out int sampleRate, out int bitsPerSample)
    {
        using var stream = File.OpenRead(filename);
        using var reader = new BinaryReader(stream);
        
        string signature = new string(reader.ReadChars(4));
        if (signature != "RIFF") throw new NotSupportedException("Недійсний формат файлу WAV.");
        
        reader.ReadInt32(); 
        
        string format = new string(reader.ReadChars(4));
        if (format != "WAVE") throw new NotSupportedException("Файл не є WAVE-потоком.");

        string formatSignature = new string(reader.ReadChars(4));
        while (formatSignature != "fmt ")
        {
            int subChunkSize = reader.ReadInt32();
            reader.BaseStream.Seek(subChunkSize, SeekOrigin.Current);
            formatSignature = new string(reader.ReadChars(4));
        }

        int formatChunkSize = reader.ReadInt32();
        reader.ReadInt16(); 
        channels = reader.ReadInt16();
        sampleRate = reader.ReadInt32();
        reader.ReadInt32(); 
        reader.ReadInt16(); 
        bitsPerSample = reader.ReadInt16();

        if (formatChunkSize > 16) reader.BaseStream.Seek(formatChunkSize - 16, SeekOrigin.Current);

        string dataSignature = new string(reader.ReadChars(4));
        while (dataSignature != "data")
        {
            int subChunkSize = reader.ReadInt32();
            reader.BaseStream.Seek(subChunkSize, SeekOrigin.Current);
            dataSignature = new string(reader.ReadChars(4));
        }

        int dataChunkSize = reader.ReadInt32();
        audioData = reader.ReadBytes(dataChunkSize);
    }

    public void Dispose()
    {
        if (_musicSource != 0) _al.DeleteSource(_musicSource);
        foreach (var buffer in _buffers.Values) _al.DeleteBuffer(buffer);

        _alc.MakeContextCurrent(null);
        if (_context != null) _alc.DestroyContext(_context);
        if (_device != null) _alc.CloseDevice(_device);
        
        _alc.Dispose();
        _al.Dispose();
    }
}