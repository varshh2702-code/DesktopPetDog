using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DesktopPetDog;

/// <summary>Frame-by-frame puppy renderer. Walking changes the actual leg/body artwork, not just the window position.</summary>
public sealed class DogView : Grid
{
    private readonly Image _puppy=new(){Stretch=Stretch.Uniform,SnapsToDevicePixels=true};
    private readonly Border _tag=new(){Width=18,Height=18,CornerRadius=new CornerRadius(9),HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(74,112,0,0),Visibility=Visibility.Collapsed};
    private readonly BitmapSource _sitting;
    private readonly BitmapSource _sleeping;
    private readonly BitmapSource[] _walkFrames;
    private readonly DispatcherTimer _frameTimer=new(){Interval=TimeSpan.FromMilliseconds(95)};
    private int _frame;

    public DogView()
    {
        Width=300;Height=220;RenderTransformOrigin=new Point(.5,.82);
        _sitting=Load("Assets/golden-retriever-puppy.png");
        _sleeping=Load("Assets/sleeping-puppy.png");
        _walkFrames=Enumerable.Range(0,8).Select(i=>Load($"Assets/Walk/walk-{i}.png")).ToArray();
        _puppy.Source=_sitting;Children.Add(_puppy);Children.Add(_tag);
        _frameTimer.Tick+=(_,_)=>{_frame=(_frame+1)%_walkFrames.Length;_puppy.Source=_walkFrames[_frame];};
    }

    private static BitmapSource Load(string path)
    {
        var b=new BitmapImage();b.BeginInit();b.UriSource=new Uri($"pack://application:,,,/{path}",UriKind.Absolute);b.CacheOption=BitmapCacheOption.OnLoad;b.EndInit();b.Freeze();return b;
    }

    public void ApplyStyle(string colour,string accessory)
    {
        // Artwork keeps its natural cream coat. Accessories are compact tags, never bars over the puppy.
        _tag.Visibility=accessory=="None"?Visibility.Collapsed:Visibility.Visible;
        _tag.Background=accessory switch{"Bow"=>Brushes.HotPink,"Bandana"=>Brushes.MediumSeaGreen,"Hat"=>Brushes.SteelBlue,_=>Brushes.Crimson};
    }

    public void StartWalking(bool toRight)
    {
        BeginAnimation(RenderTransformProperty,null);RenderTransform=Transform.Identity;
        _puppy.LayoutTransform=new ScaleTransform(toRight?1:-1,1);_frame=0;_puppy.Source=_walkFrames[0];_frameTimer.Start();_tag.Visibility=Visibility.Collapsed;
    }

    public void StopWalking()
    {
        _frameTimer.Stop();_puppy.LayoutTransform=Transform.Identity;_puppy.Source=_sitting;
    }

    public void Animate(string action)
    {
        _frameTimer.Stop();_puppy.LayoutTransform=Transform.Identity;_puppy.Source=action=="sleep"?_sleeping:_sitting;
        BeginAnimation(RenderTransformProperty,null);
        if(action=="jump"){var t=new TranslateTransform();RenderTransform=t;t.BeginAnimation(TranslateTransform.YProperty,new DoubleAnimation(0,-34,TimeSpan.FromMilliseconds(260)){AutoReverse=true,RepeatBehavior=new RepeatBehavior(2)});}
        else if(action=="sleep"){var t=new ScaleTransform(1,1);RenderTransform=t;t.BeginAnimation(ScaleTransform.ScaleYProperty,new DoubleAnimation(.985,1.015,TimeSpan.FromSeconds(1.8)){AutoReverse=true,RepeatBehavior=RepeatBehavior.Forever});}
        else if(action=="stretch"){var t=new ScaleTransform(1,1);RenderTransform=t;t.BeginAnimation(ScaleTransform.ScaleXProperty,new DoubleAnimation(.96,1.06,TimeSpan.FromMilliseconds(500)){AutoReverse=true});}
        else{var t=new RotateTransform();RenderTransform=t;t.BeginAnimation(RotateTransform.AngleProperty,new DoubleAnimation(-3,3,TimeSpan.FromMilliseconds(180)){AutoReverse=true,RepeatBehavior=new RepeatBehavior(2)});}
    }

    public void Wag()=>Animate("happy");
    public void Blink(){if(!_frameTimer.IsEnabled)_puppy.BeginAnimation(OpacityProperty,new DoubleAnimation(1,.88,TimeSpan.FromMilliseconds(70)){AutoReverse=true});}
}
