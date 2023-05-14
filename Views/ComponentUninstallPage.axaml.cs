using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Chief_Reloaded.Views;

public partial class ComponentUninstallPage : UserControl
{
    public ComponentUninstallPage()
    {
        InitializeComponent();
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (!await Utils.CheckWoolangInstallStatus())
            {
                UninstallWoolangCButton.IsVisible = false;
            }
        });
    }

    private void ReturnIndexButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow!;
        var control = window.Find<TransitioningContentControl>("ContentControl")!;
        control.Content = new MainPage();
    }
}