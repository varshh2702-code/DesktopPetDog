using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DesktopPetDog;

/// <summary>Realistic Labrador animation player. Every frame is contained inside the pet window.</summary>
public sealed class DogView : Grid
{
    private readonly Image _puppy = new() { Stretch = Stretch.Uniform, SnapsToDevicePixels = true, Margin = new Thickness(8) };
    private readonly Canvas _hearts = new() { Width = 300, Height = 220, IsHitTestVisible = false };
    private readonly Border _tag = new() { Width = 14, Height = 14, CornerRadius = new CornerRadius(7), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(46, 40, 0, 0), Visibility = Visibility.Collapsed };
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(115) };
    private readonly BitmapSource[] _sits;
    private readonly BitmapSource[] _plays;
    private readonly BitmapSource[] _walks;
    private readonly BitmapSource _sleep;
    private readonly Dictionary<string, BitmapSource> _colourCache = new();
    private readonly Random _random = new();
    private BitmapSource _current;
    private string _colour = "Golden";
    private string _mode = "sit";
    private int _frame, _ticks, _playRow;

    public DogView()
    {
        Width = 300; Height = 220; ClipToBounds = true; RenderTransformOrigin = new Point(.5, .82);
        var fallback = Load("Assets/golden-retriever-puppy.png");
        // Animation assets are optional at runtime. A decoder/resource failure must
        // never prevent the pet window from opening.
        _sits = TryGrid("Assets/Animations/sit-poses.png", 3, 2, fallback);
        _plays = TryGrid("Assets/Animations/play-actions.png", 4, 4, fallback);
        _walks = TryGrid("Assets/Animations/walk-cycle.png", 8, 1, fallback);
        _sleep = TryLoad("Assets/sleeping-puppy.png", fallback);
        _current = _sits[0];
        Children.Add(_puppy); Children.Add(_tag); Children.Add(_hearts);
        _timer.Tick += (_, _) => NextFrame();
        Sit();
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

    private static BitmapSource[] TryGrid(string path, int columns, int rows, BitmapSource fallback)
    {
        try { return CropGrid(Load(path), columns, rows); }
        catch (Exception ex) { CrashLog.TryWrite(ex); return Enumerable.Repeat(fallback, columns * rows).ToArray(); }
    }

    private static BitmapSource[] CropGrid(BitmapSource sheet, int columns, int rows)
    {
        var width = sheet.PixelWidth / columns; var height = sheet.PixelHeight / rows;
        var result = new BitmapSource[columns * rows];
        for (var i = 0; i < result.Length; i++)
        {
            var crop = new CroppedBitmap(sheet, new Int32Rect((i % columns) * width, (i / columns) * height, width, height));
            crop.Freeze(); result[i] = crop;
        }
        return result;
    }

    public void ApplyStyle(string colour, string accessory)
    {
        _colour = colour; _colourCache.Clear(); Show(_current);
        _tag.Visibility = accessory == "None" ? Visibility.Collapsed : Visibility.Visible;
        _tag.Background = accessory switch { "Bow" => B("#ED5F9A"), "Bandana" => B("#38A878"), "Hat" => B("#477FC4"), _ => B("#C84A46") };
    }

    private void Show(BitmapSource source)
    {
        _current = source;
        if (_colour == "Golden") { _puppy.Source = source; return; }
        var key = $"{source.GetHashCode()}:{_colour}";
        if (!_colourCache.TryGetValue(key, out var changed))
        {
            var target = _colour switch { "Cream" => (242, 218, 169), "Brown" => (151, 82, 38), "Grey" => (151, 157, 163), "White" => (232, 231, 224), _ => (230, 160, 75) };
            changed = Recolour(source, target); _colourCache[key] = changed;
        }
        _puppy.Source = changed;
    }

    private static BitmapSource Recolour(BitmapSource source, (int R, int G, int B) target)
    {
        var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0); var stride = converted.PixelWidth * 4; var pixels = new byte[stride * converted.PixelHeight]; converted.CopyPixels(pixels, stride, 0);
        for (var i = 0; i < pixels.Length; i += 4)
        {
            var b = pixels[i]; var g = pixels[i + 1]; var r = pixels[i + 2]; if (pixels[i + 3] < 20) continue;
            if (r > 105 && r > g * 1.04 && g > b * 1.04)
            {
                var light = Math.Clamp((r + g + b) / 3.0 / 175.0, .38, 1.42);
                pixels[i] = (byte)Math.Clamp(target.B * light, 0, 255); pixels[i + 1] = (byte)Math.Clamp(target.G * light, 0, 255); pixels[i + 2] = (byte)Math.Clamp(target.R * light, 0, 255);
            }
        }
        var result = BitmapSource.Create(converted.PixelWidth, converted.PixelHeight, converted.DpiX, converted.DpiY, PixelFormats.Bgra32, null, pixels, stride); result.Freeze(); return result;
    }

    private static Brush B(string value) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));

    public void StartWalking(bool toRight)
    {
        ResetTransform(); LayoutTransform = new ScaleTransform(toRight ? 1 : -1, 1); _mode = "walk"; _frame = 0; _ticks = 0; _timer.Start(); NextFrame();
    }

    public void StopWalking() { _timer.Stop(); LayoutTransform = Transform.Identity; Sit(); }

    public void Sit()
    {
        _timer.Stop(); ResetTransform(); LayoutTransform = Transform.Identity; _mode = "sit"; _frame = (_frame + 1 + _random.Next(2)) % _sits.Length; Show(_sits[_frame]);
    }

    public void Play()
    {
        ResetTransform(); LayoutTransform = Transform.Identity; _mode = "play"; _playRow = (_playRow + 1) % 4; _frame = 0; _ticks = 0; _timer.Start(); NextFrame();
    }

    private void Sleep()
    {
        _timer.Stop(); ResetTransform(); LayoutTransform = Transform.Identity; _mode = "sleep"; Show(_sleep);
        var scale = new ScaleTransform(1, 1); RenderTransform = scale;
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(.985, 1.01, TimeSpan.FromSeconds(1.9)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
    }

    private void NextFrame()
    {
        _ticks++;
        if (_mode == "walk") { _frame = (_frame + 1) % _walks.Length; Show(_walks[_frame]); }
        else if (_mode == "play")
        {
            _frame = (_frame + 1) % 4; Show(_plays[_playRow * 4 + _frame]);
            if (_ticks >= 20) { _timer.Stop(); Sit(); }
        }
    }

    public void Pet()
    {
        _timer.Stop(); ResetTransform(); LayoutTransform = Transform.Identity; Show(_sits[2]); _hearts.Children.Clear();
        var starts = new[] { (72d, 72d), (108d, 43d), (145d, 59d), (182d, 42d), (216d, 74d) };
        for (var i = 0; i < starts.Length; i++)
        {
            var heart = new TextBlock { Text = "♥", FontSize = 22 + i % 2 * 7, Foreground = i % 2 == 0 ? B("#FF4F87") : B("#FF86AE"), Opacity = 0 };
            Canvas.SetLeft(heart, starts[i].Item1); Canvas.SetTop(heart, starts[i].Item2); _hearts.Children.Add(heart);
            heart.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)) { AutoReverse = true, BeginTime = TimeSpan.FromMilliseconds(i * 90), RepeatBehavior = new RepeatBehavior(2) });
            heart.BeginAnimation(Canvas.TopProperty, new DoubleAnimation(starts[i].Item2, starts[i].Item2 - 44, TimeSpan.FromSeconds(1.15)) { BeginTime = TimeSpan.FromMilliseconds(i * 90) });
        }
    }

    public void Animate(string action)
    {
        ResetTransform();
        if (action == "sleep") { Sleep(); return; }
        if (action == "play" || action == "jump") { Play(); return; }
        if (action == "sit") { Sit(); return; }
        if (action is "happy" or "wag") { Show(_sits[2]); var sway = new RotateTransform(); RenderTransform = sway; sway.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(-2.5, 2.5, TimeSpan.FromMilliseconds(170)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(3) }); return; }
        Sit();
    }

    private void ResetTransform() { BeginAnimation(RenderTransformProperty, null); RenderTransform = Transform.Identity; }
    public void Wag() => Animate("wag");
    public void Blink()
    {
        if (_timer.IsEnabled) return;
        var old = _frame; Show(_sits[5]);
        var wait = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
        wait.Tick += (_, _) => { wait.Stop(); if (_mode == "sit") Show(_sits[Math.Clamp(old, 0, _sits.Length - 1)]); };
        wait.Start();
    }
}
