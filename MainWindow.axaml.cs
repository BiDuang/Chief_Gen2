using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Chief_Reloaded.Views;
using Controls = Chief_Reloaded.Views.Controls;

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
        Controls.GetMainWindow()!.Close();
    }

    private void WindowMinimizeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Controls.GetMainWindow()!.WindowState = WindowState.Minimized;
    }
}