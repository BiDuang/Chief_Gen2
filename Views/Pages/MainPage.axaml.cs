using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;

namespace Chief_Reloaded.Views.Pages;

public partial class MainPage : UserControl
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void WoolCInstallButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow;
        var control = window.Find<TransitioningContentControl>("ContentControl");
        control.Content = new WoolCInstallPage();
    }
}