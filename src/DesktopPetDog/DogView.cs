using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DesktopPetDog;

/// <summary>Stable frame-by-frame Labrador renderer used by the earlier second version.</summary>
public sealed class DogView : Grid
{
    private readonly Image _puppy = new() { Stretch = Stretch.Uniform, SnapsToDevicePixels = true, Margin = new Thickness(5) };
    private readonly Canvas _hearts = new() { Width = 300, Height = 220, IsHitTestVisible = false };
    private readonly Border _tag = new() { Width = 18, Height = 18, CornerRadius = new CornerRadius(9), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(74, 112, 0, 0), Visibility = Visibility.Collapsed };
    private readonly BitmapSource _sitting;
    private readonly BitmapSource[] _sitFrames;
    private readonly BitmapSource[] _playFrames;
    private readonly BitmapSource _sleeping;
    private readonly BitmapSource[] _walkFrames;
    private readonly DispatcherTimer _frameTimer = new() { Interval = TimeSpan.FromMilliseconds(95) };
    private int _frame, _sitIndex = -1, _playSequence = -1, _playTicks;
    private string _mode = "sit";

    public DogView()
    {
        Width = 300; Height = 220; ClipToBounds = true; RenderTransformOrigin = new Point(.5, .82);
        _sitting = Load("Assets/golden-retriever-puppy.png");
        _sitFrames = Enumerable.Range(0, 6).Select(i => TryLoad($"Assets/Frames/Sit/{i:00}.png", _sitting)).ToArray();
        _playFrames = Enumerable.Range(0, 16).Select(i => TryLoad($"Assets/Frames/Play/{i:00}.png", _sitting)).ToArray();
        _sleeping = TryLoad("Assets/sleeping-puppy.png", _sitting);
        // Clean earlier walk cycle: no neighbouring half-faces and every tail stays attached.\n        _walkFrames = Enumerable.Range(0, 8).Select(i => TryLoad($"Assets/Walk/walk-{i}.png", _sitting)).ToArray();
        _puppy.Source = _sitting; Children.Add(_puppy); Children.Add(_tag); Children.Add(_hearts);
        _frameTimer.Tick += (_, _) => NextFrame();
    }

    private static BitmapSource Load(string path)
    {
        var image = new BitmapImage(); image.BeginInit();
        image.UriSource = new Uri($"pack://application:,,,/{path}", UriKind.Absolute);
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
        image.EndInit(); image.Freeze(); return image;
    }

    private static BitmapSource TryLoad(string path, BitmapSource fallback)
    {
        try { return Load(path); } catch (Exception ex) { CrashLog.TryWrite(ex); return fallback; }
    }

    public void ApplyStyle(string colour, string accessory)
    {
        // Keep the natural Labrador coat. The previous recolouring filter caused
        // yellow fur and runtime failures on some Windows image decoders.
        _tag.Visibility = accessory == "None" ? Visibility.Collapsed : Visibility.Visible;
        _tag.Background = accessory switch { "Bow" => Brushes.HotPink, "Bandana" => Brushes.MediumSeaGreen, "Hat" => Brushes.SteelBlue, _ => Brushes.Crimson };
    }

    public void StartWalking(bool toRight)
    {
        ResetTransform(); _puppy.LayoutTransform = new ScaleTransform(toRight ? 1 : -1, 1);
        _mode = "walk"; _frame = 0; _puppy.Source = _walkFrames[0]; _frameTimer.Interval = TimeSpan.FromMilliseconds(95); _frameTimer.Start(); _tag.Visibility = Visibility.Collapsed;
    }

    public void StopWalking()
    {
        _frameTimer.Stop(); _puppy.LayoutTransform = Transform.Identity; ResetTransform();
        _mode = "sit"; _puppy.Source = _sitFrames[Math.Max(0, _sitIndex)];
    }

    public void Sit()
    {
        _frameTimer.Stop(); _puppy.LayoutTransform = Transform.Identity; ResetTransform(); _mode = "sit";
        _sitIndex = (_sitIndex + 1) % _sitFrames.Length; _puppy.Source = _sitFrames[_sitIndex];
    }

    public void Play()
    {
        _frameTimer.Stop(); _puppy.LayoutTransform = Transform.Identity; ResetTransform(); _mode = "play";
        _playSequence = (_playSequence + 1) % 4; _frame = 0; _playTicks = 0;
        _puppy.Source = _playFrames[_playSequence * 4]; _frameTimer.Interval = TimeSpan.FromMilliseconds(135); _frameTimer.Start();
    }

    private void NextFrame()
    {
        if (_mode == "walk")
        {
            _frame = (_frame + 1) % _walkFrames.Length; _puppy.Source = _walkFrames[_frame]; return;
        }
        if (_mode == "play")
        {
            _playTicks++; _frame = (_frame + 1) % 4; _puppy.Source = _playFrames[_playSequence * 4 + _frame];
            if (_playTicks >= 15) Sit();
        }
    }

    public void Animate(string action)
    {
        if (action is "jump" or "play") { Play(); return; }
        if (action == "sit") { Sit(); return; }
        _frameTimer.Stop(); _puppy.LayoutTransform = Transform.Identity; _mode = action;
        _puppy.Source = action == "sleep" ? _sleeping : _sitFrames[2];
        ResetTransform();
        if (action == "sleep")
        {
            var breathe = new ScaleTransform(1, 1); RenderTransform = breathe;
            breathe.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(.985, 1.015, TimeSpan.FromSeconds(1.8)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
        }
        else if (action == "stretch")
        {
            var stretch = new ScaleTransform(1, 1); RenderTransform = stretch;
            stretch.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(.96, 1.06, TimeSpan.FromMilliseconds(500)) { AutoReverse = true });
        }
        else
        {
            var wag = new RotateTransform(); RenderTransform = wag;
            wag.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(-3, 3, TimeSpan.FromMilliseconds(180)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(2) });
        }
    }

    public void Pet()
    {
        _frameTimer.Stop(); _mode = "pet"; _puppy.Source = _sitFrames[2]; _hearts.Children.Clear();
        var points = new[] { (72d, 70d), (108d, 42d), (146d, 58d), (184d, 42d), (216d, 72d) };
        for (var i = 0; i < points.Length; i++)
        {
            var heart = new TextBlock { Text = "♥", FontSize = 22 + i % 2 * 6, Foreground = i % 2 == 0 ? Brushes.DeepPink : Brushes.HotPink, Opacity = 0 };
            Canvas.SetLeft(heart, points[i].Item1); Canvas.SetTop(heart, points[i].Item2); _hearts.Children.Add(heart);
            heart.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)) { AutoReverse = true, BeginTime = TimeSpan.FromMilliseconds(i * 90), RepeatBehavior = new RepeatBehavior(2) });
            heart.BeginAnimation(Canvas.TopProperty, new DoubleAnimation(points[i].Item2, points[i].Item2 - 42, TimeSpan.FromSeconds(1.1)) { BeginTime = TimeSpan.FromMilliseconds(i * 90) });
        }
    }

    private void ResetTransform() { BeginAnimation(RenderTransformProperty, null); RenderTransform = Transform.Identity; }
    public void Wag() => Animate("happy");
    public void Blink() { if (!_frameTimer.IsEnabled) _puppy.BeginAnimation(OpacityProperty, new DoubleAnimation(1, .88, TimeSpan.FromMilliseconds(70)) { AutoReverse = true }); }
}
