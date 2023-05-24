using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
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
        if (ActualThemeVariant == ThemeVariant.Dark)
            ExperimentalAcrylicBorder.Material = new ExperimentalAcrylicMaterial
            {
                BackgroundSource = AcrylicBackgroundSource.Digger,
                TintOpacity = 1,
                TintColor = Colors.Black,
                MaterialOpacity = 0.65
            };
        else
            ExperimentalAcrylicBorder.Material = new ExperimentalAcrylicMaterial
            {
                BackgroundSource = AcrylicBackgroundSource.Digger,
                TintOpacity = 1,
                TintColor = Colors.WhiteSmoke,
                MaterialOpacity = 0.65
            };

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