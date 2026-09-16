using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DesktopPetDog;

/// <summary>Main golden-retriever artwork with a code-drawn fallback if the resource cannot load.</summary>
public sealed class DogView : Grid
{
    private readonly Image _puppy = new() { Stretch=Stretch.Uniform, SnapsToDevicePixels=true };
    private readonly Canvas _fallback = new() { Width=170, Height=150, Visibility=Visibility.Collapsed };
    private readonly Border _accessory = new() { HorizontalAlignment=HorizontalAlignment.Center, VerticalAlignment=VerticalAlignment.Top, CornerRadius=new CornerRadius(8) };
    private readonly Ellipse _body=new(),_head=new(),_tail=new(),_eye1=new(),_eye2=new();
    private readonly Polygon _ear1=new(),_ear2=new();

    public DogView()
    {
        Width=190;Height=190;RenderTransformOrigin=new Point(.5,.82);
        BuildFallback();Children.Add(_fallback);Children.Add(_puppy);Children.Add(_accessory);
        try
        {
            var image=new BitmapImage();image.BeginInit();image.UriSource=new Uri("pack://application:,,,/Assets/golden-retriever-puppy.png",UriKind.Absolute);image.CacheOption=BitmapCacheOption.OnLoad;image.EndInit();image.Freeze();_puppy.Source=image;
        }
        catch { _puppy.Visibility=Visibility.Collapsed;_fallback.Visibility=Visibility.Visible; }
        ApplyStyle("Cream","Collar");
    }

    private void BuildFallback()
    {
        _tail.Width=60;_tail.Height=22;Canvas.SetLeft(_tail,15);Canvas.SetTop(_tail,78);_tail.RenderTransformOrigin=new Point(.9,.5);
        _body.Width=105;_body.Height=70;Canvas.SetLeft(_body,38);Canvas.SetTop(_body,65);
        _head.Width=82;_head.Height=75;Canvas.SetLeft(_head,72);Canvas.SetTop(_head,24);
        _ear1.Points=new PointCollection([new Point(78,45),new Point(66,8),new Point(99,34)]);_ear2.Points=new PointCollection([new Point(130,33),new Point(158,8),new Point(151,50)]);
        _eye1.Width=_eye2.Width=9;_eye1.Height=_eye2.Height=12;Canvas.SetLeft(_eye1,101);Canvas.SetLeft(_eye2,132);Canvas.SetTop(_eye1,52);Canvas.SetTop(_eye2,52);_eye1.Fill=_eye2.Fill=Brushes.Black;
        var muzzle=new Ellipse{Width=40,Height=28,Fill=new SolidColorBrush(Color.FromArgb(120,255,245,225))};Canvas.SetLeft(muzzle,108);Canvas.SetTop(muzzle,68);
        var nose=new Ellipse{Width=13,Height=10,Fill=Brushes.Black};Canvas.SetLeft(nose,125);Canvas.SetTop(nose,72);
        foreach(var shape in new Shape[]{_tail,_body,_ear1,_ear2,_head,muzzle,nose,_eye1,_eye2})_fallback.Children.Add(shape);
    }

    public void ApplyStyle(string colour,string accessory)
    {
        var brush=new SolidColorBrush(colour switch{"Cream"=>Color.FromRgb(239,220,180),"Brown"=>Color.FromRgb(125,76,52),"Grey"=>Color.FromRgb(145,150,160),"White"=>Color.FromRgb(245,245,240),_=>Color.FromRgb(211,142,75)});
        _body.Fill=_head.Fill=_tail.Fill=_ear1.Fill=_ear2.Fill=brush;
        _accessory.Width=accessory=="None"?0:60;_accessory.Height=accessory=="Hat"?20:9;_accessory.Margin=new Thickness(0,accessory=="Hat"?17:120,0,0);
        _accessory.Background=accessory switch{"Bow"=>Brushes.HotPink,"Bandana"=>Brushes.MediumSeaGreen,"Hat"=>Brushes.SteelBlue,"None"=>Brushes.Transparent,_=>Brushes.Crimson};
    }

    public void Animate(string action)
    {
        BeginAnimation(RenderTransformProperty,null);
        if(action=="jump") { var t=new TranslateTransform();RenderTransform=t;t.BeginAnimation(TranslateTransform.YProperty,new DoubleAnimation(0,-34,TimeSpan.FromMilliseconds(260)){AutoReverse=true,RepeatBehavior=new RepeatBehavior(2)}); }
        else if(action=="sleep") { var t=new ScaleTransform(1,1);RenderTransform=t;t.BeginAnimation(ScaleTransform.ScaleYProperty,new DoubleAnimation(.97,1.02,TimeSpan.FromSeconds(1.7)){AutoReverse=true,RepeatBehavior=RepeatBehavior.Forever}); }
        else if(action=="stretch") { var t=new ScaleTransform(1,1);RenderTransform=t;t.BeginAnimation(ScaleTransform.ScaleXProperty,new DoubleAnimation(.96,1.08,TimeSpan.FromMilliseconds(500)){AutoReverse=true}); }
        else { var t=new RotateTransform();RenderTransform=t;t.BeginAnimation(RotateTransform.AngleProperty,new DoubleAnimation(-4,4,TimeSpan.FromMilliseconds(190)){AutoReverse=true,RepeatBehavior=new RepeatBehavior(3)}); }
        if(action!="sleep")Wag();
    }

    public void Wag()
    {
        var r=new RotateTransform();_tail.RenderTransform=r;r.BeginAnimation(RotateTransform.AngleProperty,new DoubleAnimation(-20,25,TimeSpan.FromMilliseconds(150)){AutoReverse=true,RepeatBehavior=new RepeatBehavior(4)});
        if(_puppy.Visibility==Visibility.Visible){var bodyRock=new RotateTransform();RenderTransform=bodyRock;bodyRock.BeginAnimation(RotateTransform.AngleProperty,new DoubleAnimation(-2,2,TimeSpan.FromMilliseconds(130)){AutoReverse=true,RepeatBehavior=new RepeatBehavior(3)});}
    }

    public void Blink()
    {
        var t=new ScaleTransform(1,1);RenderTransform=t;t.BeginAnimation(ScaleTransform.ScaleYProperty,new DoubleAnimation(1,.985,TimeSpan.FromMilliseconds(80)){AutoReverse=true});
        _eye1.BeginAnimation(HeightProperty,new DoubleAnimation(12,2,TimeSpan.FromMilliseconds(90)){AutoReverse=true});_eye2.BeginAnimation(HeightProperty,new DoubleAnimation(12,2,TimeSpan.FromMilliseconds(90)){AutoReverse=true});
    }
}
