using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DesktopPetDog;

public sealed class SettingsWindow : Window
{
    private readonly SettingsStore _store;
    private readonly TextBox _name=new(), _hotkey=new();
    private readonly ComboBox _colour=new(),_accessory=new(),_moodFreq=new(),_animation=new(),_message=new();
    private readonly Slider _size=new(){Minimum=.65,Maximum=1.35,TickFrequency=.05,IsSnapToTickEnabled=true};
    private readonly CheckBox _sound=Box("Enable gentle sounds"),_startup=Box("Launch when Windows starts"),_top=Box("Stay on top"),_full=Box("Automatic full-screen quiet mode"),_hideFull=Box("Never auto-hide (use Quiet Mode during full-screen)"),_save=Box("Save preferences locally"),_break=Box("Focus break reminder");
    private readonly TextBox _focus=new(){Width=80};
    public SettingsWindow(SettingsStore store)
    {
        _store=store;Title="Desktop Pet Dog Settings";Width=600;Height=680;WindowStartupLocation=WindowStartupLocation.CenterOwner;Background=new SolidColorBrush(Color.FromRgb(250,246,240));
        var root=new DockPanel{Margin=new Thickness(18)};Content=root;var buttons=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,12,0,0)};DockPanel.SetDock(buttons,Dock.Bottom);root.Children.Add(buttons);
        var saveBtn=Button("Save",true);saveBtn.Click+=(_,_)=>Save();var cancel=Button("Cancel");cancel.Click+=(_,_)=>{DialogResult=false;Close();};buttons.Children.Add(cancel);buttons.Children.Add(saveBtn);
        var scroll=new ScrollViewer{VerticalScrollBarVisibility=ScrollBarVisibility.Auto};root.Children.Add(scroll);var panel=new StackPanel();scroll.Content=panel;
        panel.Children.Add(Heading("Appearance"));Add(panel,"Dog name",_name); Fill(_colour,["Caramel","Cream","Brown","Grey","White"]);Add(panel,"Colour",_colour);Fill(_accessory,["Collar","Bow","Bandana","Hat","None"]);Add(panel,"Accessory",_accessory);Add(panel,"Size",_size);
        _hideFull.IsChecked=true;_hideFull.IsEnabled=false;panel.Children.Add(Heading("Behaviour"));Fill(_animation,["0 - No random movement","1 - Low","2 - Balanced","3 - Playful"]);Add(panel,"Animation frequency",_animation);Fill(_message,["0 - Manual only","1 - Rare","2 - Balanced"]);Add(panel,"Message frequency",_message);Fill(_moodFreq,["Manual","Daily","Every few hours","Never"]);Add(panel,"Mood questions",_moodFreq);panel.Children.Add(_sound);panel.Children.Add(_top);panel.Children.Add(_startup);panel.Children.Add(_full);panel.Children.Add(_hideFull);
        panel.Children.Add(Heading("Focus and shortcut"));Add(panel,"Focus minutes",_focus);panel.Children.Add(_break);Add(panel,"Show/hide hotkey (example: Ctrl+Alt+D)",_hotkey);
        panel.Children.Add(Heading("Local data and privacy"));panel.Children.Add(_save);var privacy=new TextBlock{Text="Screen-blind by design: the pet does not read window titles, app names, tabs, files, clipboard, typing, camera, microphone, location, contacts, screenshots, or screen contents. It makes no network requests. Full-screen detection compares only the foreground window rectangle with its monitor rectangle.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,6,0,10)};panel.Children.Add(privacy);
        var view=Button("View Exactly What Is Stored");view.Click+=(_,_)=>ViewData();var del=Button("Delete All Pet Data");del.Click+=(_,_)=>DeleteData();panel.Children.Add(view);panel.Children.Add(del);LoadValues();
    }
    private static CheckBox Box(string text)=>new(){Content=text,Margin=new Thickness(0,4,0,4)};
    private static Button Button(string text,bool primary=false)=>new(){Content=text,Padding=new Thickness(15,7,15,7),Margin=new Thickness(5),MinWidth=85,Background=primary?Brushes.SteelBlue:Brushes.White,Foreground=primary?Brushes.White:Brushes.Black};
    private static TextBlock Heading(string text)=>new(){Text=text,FontSize=18,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,14,0,8),Foreground=Brushes.SaddleBrown};
    private static void Add(Panel p,string label,Control c){p.Children.Add(new TextBlock{Text=label,Margin=new Thickness(0,6,0,2)});c.Margin=new Thickness(0,0,0,5);c.HorizontalAlignment=HorizontalAlignment.Stretch;p.Children.Add(c);}
    private static void Fill(ComboBox c,IEnumerable<string> values){foreach(var v in values)c.Items.Add(v);}
    private void LoadValues(){var p=_store.Current;_name.Text=p.PetName;_colour.SelectedItem=p.Colour;_accessory.SelectedItem=p.Accessory;_size.Value=p.Size;_animation.SelectedIndex=Math.Clamp(p.AnimationFrequency,0,3);_message.SelectedIndex=Math.Clamp(p.MessageFrequency,0,2);_moodFreq.SelectedItem=p.MoodFrequency;_sound.IsChecked=p.SoundEnabled;_startup.IsChecked=p.StartWithWindows;_top.IsChecked=p.AlwaysOnTop;_full.IsChecked=p.FullScreenQuiet;_hideFull.IsChecked=true;_save.IsChecked=p.SaveLocally;_break.IsChecked=p.BreakReminder;_focus.Text=p.FocusMinutes.ToString();_hotkey.Text=p.HotKey;}
    private void Save(){var p=_store.Current;var trimmed=_name.Text.Trim();p.PetName=string.IsNullOrWhiteSpace(trimmed)?"Pip":trimmed[..Math.Min(24,trimmed.Length)];p.Colour=_colour.SelectedItem?.ToString()??"Caramel";p.Accessory=_accessory.SelectedItem?.ToString()??"None";p.Size=_size.Value;p.AnimationFrequency=_animation.SelectedIndex;p.MessageFrequency=_message.SelectedIndex;p.MoodFrequency=_moodFreq.SelectedItem?.ToString()??"Manual";p.SoundEnabled=_sound.IsChecked==true;p.StartWithWindows=_startup.IsChecked==true;p.AlwaysOnTop=_top.IsChecked==true;p.FullScreenQuiet=_full.IsChecked==true;p.HideDuringFullScreen=false;p.SaveLocally=_save.IsChecked==true;p.BreakReminder=_break.IsChecked==true;p.FocusMinutes=int.TryParse(_focus.Text,out var f)?Math.Clamp(f,1,240):25;p.HotKey=string.IsNullOrWhiteSpace(_hotkey.Text)?"Ctrl+Alt+D":_hotkey.Text.Trim();if(!p.SaveLocally)_store.DisableSavingAndDeleteFiles();else _store.Save();DialogResult=true;Close();}
    private void ViewData(){var p=JsonSerializer.Serialize(_store.Current,new JsonSerializerOptions{WriteIndented=true});var history=File.Exists(DataPaths.History)?File.ReadAllText(DataPaths.History):"(no shuffle history saved)";new TextViewer("Stored Pet Data",$"Folder: {DataPaths.Root}\n\nSETTINGS\n{p}\n\nSHUFFLE HISTORY\n{history}"){Owner=this}.ShowDialog();}
    private void DeleteData(){if(MessageBox.Show(this,"Delete preferences, position, mood answer, histories, cache, and all other pet data?","Delete All Pet Data",MessageBoxButton.YesNo,MessageBoxImage.Warning)==MessageBoxResult.Yes){_store.DeleteAll();LoadValues();MessageBox.Show(this,"All locally stored pet data was deleted.");}}
}

public sealed class MoodWindow : Window
{
    public Mood? Selected{get;private set;} public bool SaveAnswer{get;private set;}
    public MoodWindow(PetSettings settings)
    {
        Title="Mood Check-in";Width=460;Height=570;WindowStartupLocation=WindowStartupLocation.CenterScreen;ResizeMode=ResizeMode.NoResize;
        var p=new StackPanel{Margin=new Thickness(20)};var scroll=new ScrollViewer{VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Content=p};Content=scroll;p.Children.Add(new TextBlock{Text="How are you feeling right now?",FontSize=20,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,12)});
        foreach(var m in Enum.GetValues<Mood>()){var b=new Button{Content=m==Mood.Neutral?"Okay / neutral":m.ToString(),Margin=new Thickness(2),Padding=new Thickness(8)};b.Click+=(_,_)=>Choose(m);p.Children.Add(b);}
        var row=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Center,Margin=new Thickness(0,10,0,0)};p.Children.Add(row);
        foreach(var text in new[]{"Skip","Not now","Don’t ask again today"}){var b=new Button{Content=text,Margin=new Thickness(3),Padding=new Thickness(7)};b.Click+=(_,_)=>{if(text=="Not now")settings.DontAskMoodUntil=DateTime.Now.AddHours(1);else if(text.StartsWith("Don"))settings.DontAskMoodUntil=DateTime.Today.AddDays(1);DialogResult=false;Close();};row.Children.Add(b);}
        Loaded+=(_,_)=>{var a=SystemParameters.WorkArea;MaxHeight=Math.Max(360,a.Height-30);Left=Math.Clamp(Left,a.Left+8,a.Right-ActualWidth-8);Top=Math.Clamp(Top,a.Top+8,a.Bottom-ActualHeight-8);};
    }
    private void Choose(Mood m){Selected=m;var r=MessageBox.Show(this,"Save this mood answer locally? Choose No to use it only for this moment.","Optional local saving",MessageBoxButton.YesNoCancel,MessageBoxImage.Question);if(r==MessageBoxResult.Cancel)return;SaveAnswer=r==MessageBoxResult.Yes;DialogResult=true;Close();}
}

public sealed class TextViewer : Window
{
    public TextViewer(string title,string text){Title=title;Width=650;Height=520;WindowStartupLocation=WindowStartupLocation.CenterOwner;Content=new TextBox{Text=text,IsReadOnly=true,TextWrapping=TextWrapping.NoWrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,FontFamily=new FontFamily("Consolas"),Margin=new Thickness(10)};}
}
