using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DesktopPetDog;

/// <summary>Polished hand-drawn puppy rendered from transparent animation frames.</summary>
public sealed class DogView : Grid
{
    private readonly Image _puppy=new(){Stretch=Stretch.Uniform,SnapsToDevicePixels=true};
    private readonly Canvas _hearts=new(){Width=300,Height=220,IsHitTestVisible=false};
    private readonly Border _tag=new(){Width=14,Height=14,CornerRadius=new CornerRadius(7),HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(48,38,0,0),Visibility=Visibility.Collapsed};
    private readonly DispatcherTimer _timer=new(){Interval=TimeSpan.FromMilliseconds(125)};
    private readonly BitmapSource[] _original=new BitmapSource[16];
    private BitmapSource[] _frames=new BitmapSource[16];
    private string _mode="sit";
    private int _index,_ticks,_sit=-1,_sleep=-1;

    public DogView()
    {
        Width=300;Height=220;RenderTransformOrigin=new Point(.5,.82);
        var sheet=Load("Assets/Cartoon/dog-sprite-sheet.png");
        for(var i=0;i<16;i++)
        {
            var crop=new CroppedBitmap(sheet,new Int32Rect((i%4)*350,(i/4)*280,350,280));crop.Freeze();_original[i]=crop;
        }
        Array.Copy(_original,_frames,16);Children.Add(_puppy);Children.Add(_tag);Children.Add(_hearts);
        _timer.Tick+=(_,_)=>NextFrame();Sit();
    }

    private static BitmapSource Load(string path)
    {
        var b=new BitmapImage();b.BeginInit();b.UriSource=new Uri($"pack://application:,,,/{path}",UriKind.Absolute);b.CacheOption=BitmapCacheOption.OnLoad;b.EndInit();b.Freeze();return b;
    }

    public void ApplyStyle(string colour,string accessory)
    {
        try
        {
            var target=colour switch
            {
                "Cream"=>(242,218,169),"Brown"=>(151,82,38),"Grey"=>(151,157,163),"White"=>(232,231,224),_=>(230,160,75)
            };
            _frames=_original.Select(x=>Recolour(x,target)).ToArray();
            _puppy.Source=_frames[Math.Clamp(_index,0,15)];
        }
        catch { Array.Copy(_original,_frames,16);_puppy.Source=_frames[Math.Clamp(_index,0,15)]; }
        _tag.Visibility=accessory=="None"?Visibility.Collapsed:Visibility.Visible;
        _tag.Background=accessory switch{"Bow"=>B("#ED5F9A"),"Bandana"=>B("#38A878"),"Hat"=>B("#477FC4"),_=>B("#C84A46")};
    }

    private static BitmapSource Recolour(BitmapSource source,(int R,int G,int B) target)
    {
        var converted=new FormatConvertedBitmap(source,PixelFormats.Bgra32,null,0);var stride=converted.PixelWidth*4;var pixels=new byte[stride*converted.PixelHeight];converted.CopyPixels(pixels,stride,0);
        for(var i=0;i<pixels.Length;i+=4)
        {
            var b=pixels[i];var g=pixels[i+1];var r=pixels[i+2];if(pixels[i+3]<8)continue;
            // Recolour warm-golden fur only; preserve cream chest, outline, eyes, tongue and ball.
            if(r>115&&r>g*1.08&&g>b*1.08)
            {
                var light=Math.Clamp((r+g+b)/3.0/170.0,.42,1.38);
                pixels[i]=(byte)Math.Clamp(target.B*light,0,255);pixels[i+1]=(byte)Math.Clamp(target.G*light,0,255);pixels[i+2]=(byte)Math.Clamp(target.R*light,0,255);
            }
        }
        var result=BitmapSource.Create(converted.PixelWidth,converted.PixelHeight,converted.DpiX,converted.DpiY,PixelFormats.Bgra32,null,pixels,stride);result.Freeze();return result;
    }

    private static Brush B(string value)=>new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
    private void Show(int index){_index=index;_puppy.Source=_frames[index];}

    public void StartWalking(bool toRight)
    {
        BeginAnimation(RenderTransformProperty,null);LayoutTransform=new ScaleTransform(toRight?1:-1,1);_mode="walk";_index=3;_ticks=0;_timer.Start();NextFrame();
    }

    public void StopWalking(){_timer.Stop();LayoutTransform=Transform.Identity;_mode="sit";Show(Math.Max(0,_sit));}
    public void Sit(){_timer.Stop();LayoutTransform=Transform.Identity;_mode="sit";_sit=(_sit+1)%4;Show(_sit);}
    public void Play(){LayoutTransform=Transform.Identity;_mode="play";_index=7;_ticks=0;_timer.Start();NextFrame();}

    private void Sleep()
    {
        _timer.Stop();LayoutTransform=Transform.Identity;_mode="sleep";_sleep=(_sleep+1)%4;Show(12+_sleep);
        var t=new ScaleTransform(1,1);RenderTransform=t;t.BeginAnimation(ScaleTransform.ScaleYProperty,new DoubleAnimation(.99,1.012,TimeSpan.FromSeconds(1.8)){AutoReverse=true,RepeatBehavior=RepeatBehavior.Forever});
    }

    private void NextFrame()
    {
        _ticks++;
        if(_mode=="walk"){_index=4+((_index-3)%4);Show(_index);}
        else if(_mode=="play")
        {
            _index=8+((_index-7)%4);Show(_index);
            if(_ticks>=32){_timer.Stop();Sit();}
        }
    }

    public void Pet()
    {
        _timer.Stop();LayoutTransform=Transform.Identity;Show(3);_hearts.Children.Clear();
        var starts=new[]{(82d,73d),(111d,46d),(145d,61d),(181d,43d),(211d,76d)};
        for(var i=0;i<starts.Length;i++)
        {
            var heart=new TextBlock{Text="♥",FontSize=22+i%2*7,Foreground=i%2==0?B("#FF4F87"):B("#FF86AE"),Opacity=0};
            Canvas.SetLeft(heart,starts[i].Item1);Canvas.SetTop(heart,starts[i].Item2);_hearts.Children.Add(heart);
            heart.BeginAnimation(OpacityProperty,new DoubleAnimation(0,1,TimeSpan.FromMilliseconds(220)){AutoReverse=true,BeginTime=TimeSpan.FromMilliseconds(i*90),RepeatBehavior=new RepeatBehavior(2)});
            heart.BeginAnimation(Canvas.TopProperty,new DoubleAnimation(starts[i].Item2,starts[i].Item2-42,TimeSpan.FromSeconds(1.15)){BeginTime=TimeSpan.FromMilliseconds(i*90)});
        }
    }

    public void Animate(string action)
    {
        BeginAnimation(RenderTransformProperty,null);
        if(action=="sleep"){Sleep();return;}if(action=="play"){Play();return;}if(action=="sit"){Sit();return;}
        _timer.Stop();LayoutTransform=Transform.Identity;Show(action=="jump"?3:Math.Max(0,_sit));
        if(action=="jump"){var t=new TranslateTransform();RenderTransform=t;t.BeginAnimation(TranslateTransform.YProperty,new DoubleAnimation(0,-32,TimeSpan.FromMilliseconds(250)){AutoReverse=true,RepeatBehavior=new RepeatBehavior(2)});}
        else if(action=="stretch"){var t=new ScaleTransform(1,1);RenderTransform=t;t.BeginAnimation(ScaleTransform.ScaleXProperty,new DoubleAnimation(.97,1.05,TimeSpan.FromMilliseconds(450)){AutoReverse=true});}
        else{var t=new RotateTransform();RenderTransform=t;t.BeginAnimation(RotateTransform.AngleProperty,new DoubleAnimation(-3,3,TimeSpan.FromMilliseconds(170)){AutoReverse=true,RepeatBehavior=new RepeatBehavior(2)});}
    }

    public void Wag()=>Animate("happy");
    public void Blink(){if(!_timer.IsEnabled)_puppy.BeginAnimation(OpacityProperty,new DoubleAnimation(1,.9,TimeSpan.FromMilliseconds(65)){AutoReverse=true});}
}
