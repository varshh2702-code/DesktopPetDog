using System.Threading;
using System.Windows;

namespace DesktopPetDog;

public partial class App : Application
{
    private Mutex? _singleInstance;
    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstance = new Mutex(true, "Local\\DesktopPetDog.SingleInstance", out var first);
        if (!first) { MessageBox.Show("Desktop Pet Dog is already running.", "Desktop Pet Dog"); Shutdown(); return; }
        DispatcherUnhandledException += (_, x) => { CrashLog.TryWrite(x.Exception); MessageBox.Show("The pet recovered from an unexpected error. Details were saved locally.", "Desktop Pet Dog"); x.Handled = true; };
        base.OnStartup(e);
        var services = new AppServices();
        var window = new MainWindow(services);
        MainWindow = window;
        window.Show();
    }
    protected override void OnExit(ExitEventArgs e) { _singleInstance?.ReleaseMutex(); _singleInstance?.Dispose(); base.OnExit(e); }
}
