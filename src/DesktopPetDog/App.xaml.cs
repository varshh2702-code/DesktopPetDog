using System.Threading;
using System.Windows;

namespace DesktopPetDog;

public partial class App : Application
{
    private Mutex? _singleInstance;
    private EventWaitHandle? _summonSignal;
    private volatile bool _exiting;
    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstance = new Mutex(true, "Local\\DesktopPetDog.SingleInstance", out var first);
        if (!first)
        {
            try { EventWaitHandle.OpenExisting("Local\\DesktopPetDog.Summon").Set(); } catch { }
            Shutdown();return;
        }
        _summonSignal=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\DesktopPetDog.Summon");
        DispatcherUnhandledException += (_, x) => { CrashLog.TryWrite(x.Exception); MessageBox.Show("The pet recovered from an unexpected error. Details were saved locally.", "Desktop Pet Dog"); x.Handled = true; };
        base.OnStartup(e);
        var services = new AppServices();
        var window = new MainWindow(services);
        MainWindow = window;
        window.Show();
        _=Task.Run(()=>{while(!_exiting){_summonSignal.WaitOne();if(!_exiting)Dispatcher.Invoke(window.SummonPet);}});
    }
    protected override void OnExit(ExitEventArgs e) { _exiting=true;_summonSignal?.Set();_summonSignal?.Dispose();_singleInstance?.ReleaseMutex();_singleInstance?.Dispose();base.OnExit(e); }
}
