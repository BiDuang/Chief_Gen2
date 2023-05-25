using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Chief_Reloaded.Views;

public partial class MainPage : UserControl
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void WoolCInstallButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Controls.PageSwitch(this, new WoolCInstallPage());
    }

    private void ComponentUninstallButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Controls.PageSwitch(this, new ComponentUninstallPage());
    }
}
