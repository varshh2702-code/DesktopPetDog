using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace DesktopPetDog;

public sealed class MainWindow : Window
{
    private readonly AppServices _s;
    private readonly DogView _dog = new();
    private readonly Border _bubble = new();
    private readonly TextBlock _bubbleText = new();
    private readonly DispatcherTimer _activity = new(), _blink = new(), _fullScreen = new();
    private readonly Random _random = new();
    private readonly Forms.NotifyIcon _tray;
    private PetMode _mode;
    private bool _dragged, _hiddenByFullScreen;
    private Point _dragStart;

    public MainWindow(AppServices services)
    {
        _s=services; Title="Desktop Pet Dog"; Width=260; Height=225; AllowsTransparency=true; Background=Brushes.Transparent;
        WindowStyle=WindowStyle.None; ResizeMode=ResizeMode.NoResize; ShowInTaskbar=false; Topmost=_s.Settings.Current.AlwaysOnTop;
        var root=new Grid(); Content=root;
        _bubble.Background=new SolidColorBrush(Color.FromArgb(245,255,255,255)); _bubble.BorderBrush=new SolidColorBrush(Color.FromRgb(99,80,70)); _bubble.BorderThickness=new(1.2); _bubble.CornerRadius=new(14); _bubble.Padding=new(12); _bubble.Margin=new(4); _bubble.VerticalAlignment=VerticalAlignment.Top; _bubble.MaxWidth=248; _bubble.Visibility=Visibility.Collapsed; _bubble.Child=_bubbleText;
        _bubbleText.TextWrapping=TextWrapping.Wrap; _bubbleText.FontSize=13; _bubbleText.Foreground=Brushes.Black;
        _dog.HorizontalAlignment=HorizontalAlignment.Center; _dog.VerticalAlignment=VerticalAlignment.Bottom; _dog.Margin=new(0,0,0,3);
        root.Children.Add(_bubble); root.Children.Add(_dog);
        _dog.ApplyStyle(_s.Settings.Current.Colour,_s.Settings.Current.Accessory); _dog.LayoutTransform=new ScaleTransform(_s.Settings.Current.Size,_s.Settings.Current.Size);
        Loaded+=OnLoaded; Closing+=OnClosing; MouseLeftButtonDown+=Down; MouseMove+=Move; MouseLeftButtonUp+=Up; MouseRightButtonUp+=(_,e)=>{ BuildPetMenu().IsOpen=true; e.Handled=true; }; Deactivated+=(_,_)=>HideBubble();
        _activity.Interval=TimeSpan.FromSeconds(15); _activity.Tick+=(_,_)=>AmbientAction();
        _blink.Interval=TimeSpan.FromSeconds(4); _blink.Tick+=(_,_)=>_dog.Blink();
        _fullScreen.Interval=TimeSpan.FromSeconds(2); _fullScreen.Tick+=(_,_)=>CheckFullScreen();
        _tray=BuildTray();
    }
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RestorePosition(); _activity.Start(); _blink.Start(); _fullScreen.Start(); _tray.Visible=true;
        _s.HotKey.Pressed+=ToggleVisible; _s.HotKey.Register(this,_s.Settings.Current.HotKey);
        ShowMessage($"Hi! I’m {_s.Settings.Current.PetName}. Right-click me for options.",6000); _dog.Animate("enter");
    }
    private void OnClosing(object? sender,System.ComponentModel.CancelEventArgs e)
    {
        if (System.Windows.Application.Current.ShutdownMode!=ShutdownMode.OnExplicitShutdown) return;
        e.Cancel=true; Hide();
    }
    private void ExitApp()
    {
        SavePosition(); _activity.Stop(); _blink.Stop(); _fullScreen.Stop(); _s.HotKey.Dispose(); _tray.Visible=false; _tray.Dispose();
        System.Windows.Application.Current.ShutdownMode=ShutdownMode.OnMainWindowClose; Close(); System.Windows.Application.Current.Shutdown();
    }
    private Forms.NotifyIcon BuildTray()
    {
        var n=new Forms.NotifyIcon{Text="Desktop Pet Dog",Icon=System.Drawing.SystemIcons.Application};
        var m=new Forms.ContextMenuStrip();
        void Add(string text,Action a)=>m.Items.Add(text,null,(_,_)=>Dispatcher.Invoke(a));
        Add("Show Pet",()=>{Show();Activate();}); Add("Hide Pet",Hide); m.Items.Add(new Forms.ToolStripSeparator());
        Add("Quiet Mode",()=>SetMode(PetMode.Quiet)); Add("Focus Mode",()=>SetMode(PetMode.Focus)); Add("Pause Pet",()=>SetMode(PetMode.Paused));
        Add("Settings",OpenSettings); m.Items.Add(new Forms.ToolStripSeparator()); Add("Exit",ExitApp); n.ContextMenuStrip=m; n.DoubleClick+=(_,_)=>Dispatcher.Invoke(ToggleVisible); return n;
    }
    private ContextMenu BuildPetMenu()
    {
        var m=new ContextMenu();
        void Add(string text,Action a){var item=new MenuItem{Header=text}; item.Click+=(_,_)=>a(); m.Items.Add(item);}
        Add("Talk to Pet",()=>ShowMessage("What would help: a joke, encouragement, a compliment, or quiet company?"));
        Add("Mood Check-in",MoodCheck); Add("Tell Me a Joke",()=>Message("clean")); Add("Dad Joke",()=>Message("dad")); Add("Encourage Me",()=>Message("motivation")); Add("Give Me a Compliment",()=>Message("compliment"));
        m.Items.Add(new Separator()); Add("Play",()=>_dog.Animate("jump")); Add("Sit",()=>_dog.Animate("sit")); Add("Sleep",()=>_dog.Animate("sleep"));
        m.Items.Add(new Separator()); Add("Focus Mode",()=>SetMode(PetMode.Focus)); Add("Quiet Mode",()=>SetMode(PetMode.Quiet)); Add("Do Not Disturb",()=>SetMode(PetMode.DoNotDisturb)); Add("Pause Pet",()=>SetMode(PetMode.Paused)); Add(_s.Settings.Current.StayHere?"Allow Movement":"Stay Here",()=>{_s.Settings.Current.StayHere=!_s.Settings.Current.StayHere;_s.Settings.Save();});
        Add("Hide Pet",Hide); m.Items.Add(new Separator()); Add("Customise",OpenSettings); Add("Settings",OpenSettings); Add("Exit",ExitApp); return m;
    }
    private void Message(string category)=>ShowMessage(_s.Shuffle.Next(category,MessageLibrary.ByCategory(category)));
    private async void ShowMessage(string text,int milliseconds=8000)
    {
        if (_mode==PetMode.DoNotDisturb) return; _bubbleText.Text=text; _bubble.Visibility=Visibility.Visible; await Task.Delay(milliseconds); if (_bubbleText.Text==text) HideBubble();
    }
    private void HideBubble()=>_bubble.Visibility=Visibility.Collapsed;
    private void Down(object s,MouseButtonEventArgs e)
    {
        if (e.ChangedButton!=MouseButton.Left) return; _dragStart=e.GetPosition(this); _dragged=false; CaptureMouse();
    }
    private void Move(object s,System.Windows.Input.MouseEventArgs e)
    {
        if (!IsMouseCaptured||e.LeftButton!=MouseButtonState.Pressed) return; var p=PointToScreen(e.GetPosition(this)); Left=p.X-_dragStart.X; Top=p.Y-_dragStart.Y; _dragged=true;
    }
    private void Up(object s,MouseButtonEventArgs e)
    {
        if (!IsMouseCaptured) return; ReleaseMouseCapture(); ClampToVisibleArea(); SavePosition(); if (!_dragged) { _dog.Wag(); ShowMessage($"{_s.Settings.Current.PetName} gives a happy tail wag!",3500); }
    }
    protected override void OnMouseDoubleClick(MouseButtonEventArgs e){base.OnMouseDoubleClick(e);_dog.Animate("jump");ShowMessage("Play time!",2500);}
    private void AmbientAction()
    {
        if (_mode is PetMode.Paused or PetMode.DoNotDisturb) return;
        if (ShouldAskMood()) { MoodCheck(); return; }
        if (_mode is PetMode.Quiet or PetMode.Focus) { if(_random.Next(3)==0)_dog.Animate("sleep"); return; }
        var actions=new[]{"wag","stretch","jump","look"}; _dog.Animate(actions[_random.Next(actions.Length)]);
        if (!_s.Settings.Current.StayHere && _s.Settings.Current.AnimationFrequency>0 && _random.Next(4)<=_s.Settings.Current.AnimationFrequency-1) MoveGently();
    }
    private bool ShouldAskMood()
    {
        var p=_s.Settings.Current;if(p.MoodFrequency is "Manual" or "Never")return false;if(p.DontAskMoodUntil>DateTime.Now)return false;
        if(p.LastMoodQuestionAt==null)return true;
        return p.MoodFrequency=="Daily" ? p.LastMoodQuestionAt.Value.Date<DateTime.Today : DateTime.Now-p.LastMoodQuestionAt.Value>=TimeSpan.FromHours(4);
    }
    private void MoveGently()
    {
        var area=GetWorkArea(); var target=Math.Clamp(Left+_random.Next(-90,91),area.Left,area.Right-Width); var a=new DoubleAnimation(target,TimeSpan.FromMilliseconds(900)){EasingFunction=new QuadraticEase()}; BeginAnimation(LeftProperty,a,HandoffBehavior.SnapshotAndReplace);
    }
    private Rect GetWorkArea()
    {
        var screen=Forms.Screen.FromPoint(new System.Drawing.Point((int)(Left+Width/2),(int)(Top+Height/2))); var dpi=VisualTreeHelper.GetDpi(this); var w=screen.WorkingArea; return new Rect(w.Left/dpi.DpiScaleX,w.Top/dpi.DpiScaleY,w.Width/dpi.DpiScaleX,w.Height/dpi.DpiScaleY);
    }
    private void ClampToVisibleArea(){var a=GetWorkArea();Left=Math.Clamp(Left,a.Left,a.Right-Width);Top=Math.Clamp(Top,a.Top,a.Bottom-Height);}
    private void RestorePosition(){var p=_s.Settings.Current;if(double.IsNaN(p.Left)||double.IsNaN(p.Top)){var a=SystemParameters.WorkArea;Left=a.Right-Width-20;Top=a.Bottom-Height-20;}else{Left=p.Left;Top=p.Top;}ClampToVisibleArea();}
    private void SavePosition(){_s.Settings.Current.Left=Left;_s.Settings.Current.Top=Top;_s.Settings.Save();}
    private void ToggleVisible(){if(IsVisible)Hide();else{Show();Activate();ClampToVisibleArea();}}
    private void SetMode(PetMode mode)
    {
        _mode=mode; if(mode==PetMode.Normal){_activity.Start();ShowMessage("Normal mode is back.");}
        else if(mode==PetMode.Focus){_activity.Start();var a=GetWorkArea();Left=a.Right-Width-12;Top=a.Bottom-Height-12;_s.Settings.Current.FocusEndsAt=DateTime.Now.AddMinutes(_s.Settings.Current.FocusMinutes);ShowMessage($"Focus mode for {_s.Settings.Current.FocusMinutes} minutes. I’ll stay quietly here.");}
        else if(mode==PetMode.DoNotDisturb){HideBubble();_activity.Stop();_dog.Animate("sleep");}
        else if(mode==PetMode.Paused){HideBubble();_activity.Stop();}
        else{_activity.Start();_dog.Animate("sleep");ShowMessage("Quiet mode is on. Manual interactions still work.");}
    }
    private void CheckFullScreen()
    {
        if(!_s.Settings.Current.FullScreenQuiet)return; var full=_s.FullScreen.IsFullScreen();
        if(full&&!_hiddenByFullScreen){_hiddenByFullScreen=true;if(_s.Settings.Current.HideDuringFullScreen)Hide();else SetMode(PetMode.Quiet);}
        else if(!full&&_hiddenByFullScreen){_hiddenByFullScreen=false;if(!IsVisible)Show();SetMode(PetMode.Normal);}
        if(_mode==PetMode.Focus&&_s.Settings.Current.FocusEndsAt<=DateTime.Now){SetMode(PetMode.Normal);if(_s.Settings.Current.BreakReminder)ShowMessage("Focus session complete. A short stretch or water break is available.");}
    }
    private void MoodCheck()
    {
        _s.Settings.Current.LastMoodQuestionAt=DateTime.Now;_s.Settings.Save();
        var d=new MoodWindow(_s.Settings.Current){Owner=this}; if(d.ShowDialog()!=true||d.Selected==null){_s.Settings.Save();return;}
        if(d.SaveAnswer){_s.Settings.Current.LastMood=d.Selected;_s.Settings.Save();} ApplyMood(d.Selected.Value);
    }
    private void ApplyMood(Mood mood)
    {
        switch(mood){case Mood.Happy:_dog.Animate("jump");Message("dog");break;case Mood.Tired:SetMode(PetMode.Quiet);ShowMessage("A slower pace is okay. Choose one small next step.");break;case Mood.Stressed:SetMode(PetMode.Quiet);ShowMessage("Try one gentle breath: in slowly, then out slowly. Skip this if it is not useful.");break;case Mood.Sad:SetMode(PetMode.Quiet);ShowMessage("A difficult moment does not erase your progress. Be gentle with the next step.");break;case Mood.Focused:SetMode(PetMode.Focus);break;case Mood.Bored:_dog.Animate("jump");Message("school");break;default:Message("positive");break;}
    }
    private void OpenSettings()
    {
        var oldStartup=_s.Settings.Current.StartWithWindows; var w=new SettingsWindow(_s.Settings){Owner=this}; if(w.ShowDialog()!=true)return;
        var p=_s.Settings.Current; Topmost=p.AlwaysOnTop; _dog.ApplyStyle(p.Colour,p.Accessory);_dog.LayoutTransform=new ScaleTransform(p.Size,p.Size);
        if(oldStartup!=p.StartWithWindows)_s.Startup.Set(p.StartWithWindows); _s.HotKey.Register(this,p.HotKey); ClampToVisibleArea();
    }
}
