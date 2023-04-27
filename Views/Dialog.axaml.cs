using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Chief_Reloaded.Views;

public partial class Dialog : Window
{
    public Dialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}