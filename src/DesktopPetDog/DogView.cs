using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DesktopPetDog;

/// <summary>Transparent vector puppy with independently animated body parts.</summary>
public sealed class DogView : Grid
{
    private readonly Canvas _scene=new(){Width=300,Height=220};
    private readonly Canvas _hearts=new(){Width=300,Height=220,IsHitTestVisible=false};
    private readonly DispatcherTimer _timer=new(){Interval=TimeSpan.FromMilliseconds(75)};
    private readonly Ellipse _body=Part(142,91),_chest=Part(72,82),_head=Part(92,82),_muzzle=Part(56,35);
    private readonly Ellipse _earL=Part(32,62),_earR=Part(32,62),_eyeL=Part(10,13),_eyeR=Part(10,13),_nose=Part(19,14);
    private readonly Ellipse[] _legs={Part(31,70),Part(31,70),Part(31,70),Part(31,70)};
    private readonly Border _tail=new(){Width=86,Height=26,CornerRadius=new CornerRadius(18),RenderTransformOrigin=new Point(.85,.5)};
    private readonly Ellipse _ball=new(){Width=38,Height=38,Fill=B("#378FE9"),Stroke=Brushes.White,StrokeThickness=2,Visibility=Visibility.Collapsed};
    private readonly Border _tag=new(){Width=15,Height=15,CornerRadius=new CornerRadius(8),Visibility=Visibility.Collapsed};
    private string _mode="sit";
    private int _tick,_sleepPose,_sitPose,_playTicks;
    private bool _right=true;

    public DogView()
    {
        Width=300;Height=220;RenderTransformOrigin=new Point(.5,.82);
        Children.Add(_scene);Children.Add(_hearts);_scene.Children.Add(_tail);
        foreach(var leg in _legs)_scene.Children.Add(leg);
        foreach(var p in new UIElement[]{_body,_chest,_head,_earL,_earR,_muzzle,_eyeL,_eyeR,_nose,_tag,_ball})_scene.Children.Add(p);
        _timer.Tick+=(_,_)=>Frame();ApplyStyle("Caramel","None");ShowSitting(0);
    }

    private static Ellipse Part(double w,double h)=>new(){Width=w,Height=h,Stroke=B("#55402A"),StrokeThickness=1.2};
    private static Brush B(string hex)=>new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    private static void At(UIElement e,double x,double y){Canvas.SetLeft(e,x);Canvas.SetTop(e,y);}
    private static void Turn(UIElement e,double a,double x=.5,double y=.5){e.RenderTransformOrigin=new Point(x,y);e.RenderTransform=new RotateTransform(a);}
    private void EyesOpen(bool open){_eyeL.Height=_eyeR.Height=open?13:4;}

    public void ApplyStyle(string colour,string accessory)
    {
        var colours=colour switch
        {
            "Cream"=>(B("#F4DCA8"),B("#FFF3D2"),B("#D9A868")),
            "Brown"=>(B("#A9602A"),B("#E8B778"),B("#73401F")),
            "Grey"=>(B("#9AA1A8"),B("#E2E5E8"),B("#69727B")),
            "White"=>(B("#F2F2EE"),B("#FFFFFF"),B("#D4D1C8")),
            _=>(B("#E6A24A"),B("#FFE0A3"),B("#B96F32"))
        };
        _body.Fill=_head.Fill=_tail.Background=colours.Item1;foreach(var l in _legs)l.Fill=colours.Item1;
        _chest.Fill=_muzzle.Fill=colours.Item2;_earL.Fill=_earR.Fill=colours.Item3;_eyeL.Fill=_eyeR.Fill=_nose.Fill=B("#24201E");
        _tag.Visibility=accessory=="None"?Visibility.Collapsed:Visibility.Visible;
        _tag.Background=accessory switch{"Bow"=>B("#ED5F9A"),"Bandana"=>B("#38A878"),"Hat"=>B("#477FC4"),_=>B("#C84A46")};
    }

    public void StartWalking(bool toRight){_right=toRight;_mode="walk";_tick=0;_playTicks=0;_ball.Visibility=Visibility.Collapsed;EyesOpen(true);_timer.Start();}
    public void StopWalking(){_timer.Stop();_mode="sit";ShowSitting(_sitPose);}
    public void Play(){_mode="play";_tick=0;_playTicks=0;_ball.Visibility=Visibility.Visible;EyesOpen(true);_timer.Start();}
    public void Sit(){_timer.Stop();_mode="sit";_sitPose=(_sitPose+1)%3;ShowSitting(_sitPose);}

    public void Pet()
    {
        Animate("happy");_hearts.Children.Clear();
        var starts=new[]{(104d,72d),(132d,48d),(164d,65d),(191d,43d),(214d,76d)};
        for(var i=0;i<starts.Length;i++)
        {
            var h=new TextBlock{Text="♥",FontSize=22+i%2*7,Foreground=i%2==0?B("#FF4F87"):B("#FF79A5"),Opacity=0};
            At(h,starts[i].Item1,starts[i].Item2);_hearts.Children.Add(h);
            h.BeginAnimation(OpacityProperty,new DoubleAnimation(0,1,TimeSpan.FromMilliseconds(220)){AutoReverse=true,BeginTime=TimeSpan.FromMilliseconds(i*90),RepeatBehavior=new RepeatBehavior(2)});
            h.BeginAnimation(Canvas.TopProperty,new DoubleAnimation(starts[i].Item2,starts[i].Item2-42,TimeSpan.FromSeconds(1.15)){BeginTime=TimeSpan.FromMilliseconds(i*90)});
        }
    }

    public void Animate(string action)
    {
        _timer.Stop();_ball.Visibility=Visibility.Collapsed;BeginAnimation(RenderTransformProperty,null);
        if(action=="sleep"){_mode="sleep";_sleepPose=(_sleepPose+1)%3;ShowSleeping(_sleepPose);return;}
        if(action=="play"){Play();return;} if(action=="sit"){Sit();return;}
        _mode="sit";ShowSitting(_sitPose);
        if(action=="jump"){var t=new TranslateTransform();RenderTransform=t;t.BeginAnimation(TranslateTransform.YProperty,new DoubleAnimation(0,-35,TimeSpan.FromMilliseconds(260)){AutoReverse=true,RepeatBehavior=new RepeatBehavior(2)});}
        else if(action=="stretch"){var t=new ScaleTransform(1,1);RenderTransform=t;t.BeginAnimation(ScaleTransform.ScaleXProperty,new DoubleAnimation(.96,1.07,TimeSpan.FromMilliseconds(500)){AutoReverse=true});}
        else{var t=new RotateTransform();RenderTransform=t;t.BeginAnimation(RotateTransform.AngleProperty,new DoubleAnimation(-3,3,TimeSpan.FromMilliseconds(170)){AutoReverse=true,RepeatBehavior=new RepeatBehavior(2)});}
    }

    private void Frame()
    {
        _tick++;if(_mode=="walk")ShowWalking(_tick*.72);
        else if(_mode=="play"){ShowPlaying(_tick*.42);if(++_playTicks>=82){_timer.Stop();_mode="sit";_ball.Visibility=Visibility.Collapsed;ShowSitting(_sitPose);}}
    }

    private void ShowSitting(int pose)
    {
        EyesOpen(true);_scene.RenderTransform=Transform.Identity;
        var lean=pose==1?-9:pose==2?8:0;var headY=pose==2?55:48;
        At(_tail,34,128);Turn(_tail,-22+(pose-1)*13,.85,.5);At(_body,73,105);At(_chest,112+lean/2,111);At(_head,105+lean,headY);Turn(_head,pose==1?-8:pose==2?9:0);
        At(_earL,99+lean,64+(headY-48));Turn(_earL,18);At(_earR,171+lean,64+(headY-48));Turn(_earR,-18);At(_muzzle,124+lean,94+(headY-48));
        At(_eyeL,125+lean,76+(headY-48));At(_eyeR,167+lean,76+(headY-48));At(_nose,143+lean,105+(headY-48));
        At(_legs[0],91,144);At(_legs[1],119,151);At(_legs[2],159,151);At(_legs[3],188,144);
        for(var i=0;i<4;i++)Turn(_legs[i],pose==1?(i<2?-6:3):pose==2?(i>1?7:-2):0);
        At(_tag,146+lean,123+(headY-48));
    }

    private void ShowWalking(double p)
    {
        var bounce=Math.Abs(Math.Sin(p))*4;_scene.RenderTransform=new ScaleTransform(_right?1:-1,1,150,110);
        At(_tail,38,101+bounce);Turn(_tail,-28+Math.Sin(p*1.7)*25,.85,.5);At(_body,73,92+bounce);At(_chest,154,96+bounce);At(_head,174,55+bounce+Math.Sin(p)*2);
        At(_earL,170,67+bounce);Turn(_earL,18+Math.Sin(p)*9);At(_earR,225,67+bounce);Turn(_earR,-12-Math.Sin(p)*7);At(_muzzle,202,94+bounce);
        At(_eyeL,195,77+bounce);At(_eyeR,226,77+bounce);At(_nose,235,101+bounce);
        var swings=new[]{Math.Sin(p),Math.Sin(p+Math.PI),Math.Sin(p+Math.PI),Math.Sin(p)};var xs=new[]{91d,126d,164d,197d};
        for(var i=0;i<4;i++){At(_legs[i],xs[i]+swings[i]*9,134+bounce+Math.Max(0,swings[i])*8);Turn(_legs[i],swings[i]*22,.5,.15);}
        At(_tag,184,116+bounce);
    }

    private void ShowPlaying(double p)
    {
        _scene.RenderTransform=Transform.Identity;var push=(1+Math.Sin(p))*.5;var bob=Math.Sin(p*2)*3;
        At(_tail,39,91+bob);Turn(_tail,-36+Math.Sin(p*2)*30,.85,.5);At(_body,72,86+bob);At(_chest,151,107+bob);At(_head,167+push*15,94+bob+push*12);
        At(_earL,163+push*15,105+bob);Turn(_earL,22);At(_earR,221+push*15,105+bob);Turn(_earR,-16);At(_muzzle,197+push*18,133+bob);
        At(_eyeL,190+push*16,116+bob);At(_eyeR,222+push*16,116+bob);At(_nose,231+push*20,141+bob);
        At(_legs[0],87,133+bob);At(_legs[1],126,135+bob);At(_legs[2],175+push*14,153+bob);At(_legs[3],203+push*19,151+bob);Turn(_legs[2],65,.5,.15);Turn(_legs[3],70,.5,.15);
        At(_ball,244+Math.Sin(p)*13,171+Math.Abs(Math.Cos(p))*5);At(_tag,184,128+bob);
    }

    private void ShowSleeping(int pose)
    {
        EyesOpen(false);_scene.RenderTransform=Transform.Identity;
        if(pose==0){At(_tail,42,145);Turn(_tail,18,.85,.5);At(_body,69,107);At(_chest,139,123);At(_head,170,119);At(_earL,164,129);At(_earR,225,128);At(_muzzle,199,153);At(_eyeL,193,143);At(_eyeR,226,143);At(_nose,231,164);At(_legs[0],91,147);At(_legs[1],120,153);At(_legs[2],179,164);At(_legs[3],206,166);}
        else if(pose==1){At(_tail,37,119);Turn(_tail,-45,.85,.5);At(_body,77,111);At(_chest,116,122);At(_head,102,103);At(_earL,93,117);At(_earR,167,116);At(_muzzle,121,142);At(_eyeL,121,130);At(_eyeR,162,130);At(_nose,142,153);At(_legs[0],79,150);At(_legs[1],110,158);At(_legs[2],162,158);At(_legs[3],193,150);}
        else{At(_tail,57,151);Turn(_tail,12,.85,.5);At(_body,80,105);At(_chest,151,119);At(_head,183,105);At(_earL,176,119);At(_earR,238,119);At(_muzzle,207,144);At(_eyeL,202,133);At(_eyeR,235,133);At(_nose,239,154);At(_legs[0],68,154);At(_legs[1],104,159);At(_legs[2],170,160);At(_legs[3],206,162);}
        foreach(var l in _legs)Turn(l,76,.5,.15);At(_tag,173,144);
    }

    public void Wag()=>Animate("happy");
    public void Blink(){if(_mode is not ("walk" or "play"))BeginAnimation(OpacityProperty,new DoubleAnimation(1,.9,TimeSpan.FromMilliseconds(70)){AutoReverse=true});}
}
