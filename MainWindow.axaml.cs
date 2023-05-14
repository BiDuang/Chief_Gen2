using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Chief_Reloaded.Views;

namespace Chief_Reloaded;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        ContentControl.Content = new MainPage();
    }

    private void WindowCloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow!.Close();
    }

    private void WindowMinimizeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow!.WindowState =
            WindowState.Minimized;
    }
}