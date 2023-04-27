using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Chief_Reloaded.Views.Pages;

public partial class WoolCInstallPage : UserControl
{
    public WoolCInstallPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ReturnIndexButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow;
        var control = window.Find<TransitioningContentControl>("ContentControl");
        control.Content = new MainPage();
    }

    private void ReturnLastButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}