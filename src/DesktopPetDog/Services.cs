using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;

namespace DesktopPetDog;

public static class DataPaths
{
    public static readonly string Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesktopPetDog");
    public static readonly string Settings = Path.Combine(Root, "settings.json");
    public static readonly string History = Path.Combine(Root, "history.json");
    public static readonly string Cache = Path.Combine(Root, "Cache");
}

public sealed class SettingsStore
{
    public PetSettings Current { get; private set; } = new();
    public SettingsStore() => Load();
    public void Load()
    {
        try { if (File.Exists(DataPaths.Settings)) Current = JsonSerializer.Deserialize(File.ReadAllText(DataPaths.Settings), PetJsonContext.Default.PetSettings) ?? new(); }
        catch { Current = new(); }
    }
    public void Save()
    {
        if (!Current.SaveLocally) return;
        Directory.CreateDirectory(DataPaths.Root);
        AtomicWrite(DataPaths.Settings, JsonSerializer.Serialize(Current, PetJsonContext.Default.PetSettings));
    }
    public static void AtomicWrite(string path, string text) { var tmp = path + ".tmp"; File.WriteAllText(tmp, text); File.Move(tmp, path, true); }
    public void DeleteAll() { try { if (Directory.Exists(DataPaths.Root)) Directory.Delete(DataPaths.Root, true); } catch { } Current = new(); }
    public void DisableSavingAndDeleteFiles() { try { if (Directory.Exists(DataPaths.Root)) Directory.Delete(DataPaths.Root, true); } catch { } Current.SaveLocally = false; }
}

public sealed class ShuffleBag
{
    private readonly SettingsStore _settings;
    private HistoryData _data = new();
    private readonly Random _random = new();
    public ShuffleBag(SettingsStore settings) { _settings = settings; Load(); }
    private void Load() { try { if (File.Exists(DataPaths.History)) _data = JsonSerializer.Deserialize(File.ReadAllText(DataPaths.History), PetJsonContext.Default.HistoryData) ?? new(); } catch { _data = new(); } }
    public string Next(string category, IReadOnlyList<string> items)
    {
        if (!_data.Remaining.TryGetValue(category, out var bag) || bag.Count == 0) bag = _data.Remaining[category] = Enumerable.Range(0, items.Count).OrderBy(_ => _random.Next()).ToList();
        var recent = _data.Recent.TryGetValue(category, out var r) ? r : _data.Recent[category] = new();
        var allowed = bag.Where(i => !recent.Contains(i)).ToList();
        var index = allowed.Count > 0 ? allowed[_random.Next(allowed.Count)] : bag[_random.Next(bag.Count)];
        bag.Remove(index); recent.Add(index); while (recent.Count > Math.Max(5, items.Count / 10)) recent.RemoveAt(0);
        Save(); return items[index];
    }
    private void Save() { if (!_settings.Current.SaveLocally) return; Directory.CreateDirectory(DataPaths.Root); SettingsStore.AtomicWrite(DataPaths.History, JsonSerializer.Serialize(_data, PetJsonContext.Default.HistoryData)); }
}

public sealed class StartupService
{
    private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    public void Set(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(KeyPath, true) ?? Registry.CurrentUser.CreateSubKey(KeyPath);
        if (enabled) key.SetValue("DesktopPetDog", $"\"{Environment.ProcessPath}\""); else key.DeleteValue("DesktopPetDog", false);
    }
}

public sealed class FullScreenDetector
{
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")] static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);
    [DllImport("user32.dll")] static extern bool GetMonitorInfo(IntPtr monitor, ref MONITORINFO info);
    [StructLayout(LayoutKind.Sequential)] struct RECT { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] struct MONITORINFO { public int Size; public RECT Monitor, Work; public uint Flags; }
    public bool IsFullScreen()
    {
        var hwnd = GetForegroundWindow(); if (hwnd == IntPtr.Zero || !GetWindowRect(hwnd, out var wr)) return false;
        var mi = new MONITORINFO { Size = Marshal.SizeOf<MONITORINFO>() }; if (!GetMonitorInfo(MonitorFromWindow(hwnd, 2), ref mi)) return false;
        return Math.Abs(wr.Left-mi.Monitor.Left)<=2 && Math.Abs(wr.Top-mi.Monitor.Top)<=2 && Math.Abs(wr.Right-mi.Monitor.Right)<=2 && Math.Abs(wr.Bottom-mi.Monitor.Bottom)<=2;
    }
}

public sealed class HotKeyService : IDisposable
{
    [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr h, int id, uint mods, uint key);
    [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr h, int id);
    private IntPtr _handle; private HwndSource? _source; public event Action? Pressed;
    public void Register(Window window, string value)
    {
        Unregister(); _handle = new WindowInteropHelper(window).Handle; _source = HwndSource.FromHwnd(_handle); _source.AddHook(Hook);
        var parts = value.Split('+'); uint mods = 0; foreach (var p in parts[..^1]) mods |= p.Trim().ToLowerInvariant() switch { "ctrl" => 2u, "alt" => 1u, "shift" => 4u, "win" => 8u, _ => 0u };
        var keyText = parts[^1].Trim().ToUpperInvariant(); uint key = keyText.Length == 1 ? (uint)keyText[0] : 0x44u; RegisterHotKey(_handle, 707, mods | 0x4000u, key);
    }
    private IntPtr Hook(IntPtr h, int msg, IntPtr wp, IntPtr lp, ref bool handled) { if (msg == 0x0312 && wp.ToInt32() == 707) { Pressed?.Invoke(); handled = true; } return IntPtr.Zero; }
    public void Unregister() { if (_handle != IntPtr.Zero) UnregisterHotKey(_handle, 707); if (_source != null) _source.RemoveHook(Hook); _source=null; _handle=IntPtr.Zero; }
    public void Dispose() => Unregister();
}

public sealed class AppServices
{
    public SettingsStore Settings { get; } = new();
    public ShuffleBag Shuffle { get; }
    public StartupService Startup { get; } = new();
    public FullScreenDetector FullScreen { get; } = new();
    public HotKeyService HotKey { get; } = new();
    public AppServices() => Shuffle = new(Settings);
}

public static class CrashLog
{
    public static void TryWrite(Exception ex) { try { Directory.CreateDirectory(DataPaths.Root); File.AppendAllText(Path.Combine(DataPaths.Root,"error.log"), $"{DateTime.UtcNow:u}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}"); } catch { } }
}
