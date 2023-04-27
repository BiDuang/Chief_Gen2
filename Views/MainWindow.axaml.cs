using Avalonia.Controls;
using Chief_Reloaded.Views.Pages;

namespace Chief_Reloaded.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ContentControl.Content = new MainPage();
    }
}