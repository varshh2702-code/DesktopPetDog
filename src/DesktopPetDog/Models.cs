using System.Text.Json.Serialization;

namespace DesktopPetDog;

public enum PetMode { Normal, Quiet, Focus, DoNotDisturb, Paused }
public enum Mood { Neutral, Happy, Tired, Stressed, Sad, Focused, Bored }

public sealed class PetSettings
{
    public string PetName { get; set; } = "Pip";
    public string Colour { get; set; } = "Caramel";
    public string Accessory { get; set; } = "Collar";
    public double Size { get; set; } = 1.0;
    public int AnimationFrequency { get; set; } = 2;
    public int MessageFrequency { get; set; } = 1;
    public string MoodFrequency { get; set; } = "Manual";
    public bool SoundEnabled { get; set; }
    public bool StartWithWindows { get; set; }
    public bool AlwaysOnTop { get; set; } = true;
    public bool FullScreenQuiet { get; set; } = true;
    public bool HideDuringFullScreen { get; set; }
    public bool SaveLocally { get; set; } = true;
    public bool StayHere { get; set; }
    public string HotKey { get; set; } = "Ctrl+Alt+D";
    public int FocusMinutes { get; set; } = 25;
    public bool BreakReminder { get; set; } = true;
    public double Left { get; set; } = double.NaN;
    public double Top { get; set; } = double.NaN;
    public DateTime? DontAskMoodUntil { get; set; }
    public DateTime? LastMoodQuestionAt { get; set; }
    public Mood? LastMood { get; set; }
    public DateTime? FocusEndsAt { get; set; }
}

public sealed class HistoryData
{
    public Dictionary<string, List<int>> Remaining { get; set; } = new();
    public Dictionary<string, List<int>> Recent { get; set; } = new();
}

[JsonSerializable(typeof(PetSettings))]
[JsonSerializable(typeof(HistoryData))]
internal partial class PetJsonContext : JsonSerializerContext { }
