using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;

namespace Chief_Reloaded.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        OnWindowMinimizeCommand = ReactiveCommand.Create(() =>
        {
            (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow
                .WindowState = WindowState.Minimized;
        });
        OnWindowCloseCommand = ReactiveCommand.Create(() =>
        {
            (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow
                .Close();
        });
    }

    public ICommand OnWindowMinimizeCommand { get; }
    public ICommand OnWindowCloseCommand { get; }
}