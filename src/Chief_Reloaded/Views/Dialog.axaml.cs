using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Chief_Reloaded.Views;

public partial class Dialog : Window
{
    public enum DialogType
    {
        Error,
        Warning,
        Information,
        Notice
    }

    public bool DialogResult;

    public Dialog()
    {
        InitializeComponent();
    }

    public void InitDialog(
        DialogType type,
        string title,
        string content,
        bool isConfirmOnly = false
    )
    {
        DialogTitle.Text = title;
        DialogContentBlock.Text = content;
        switch (type)
        {
            case DialogType.Error:
                ErrorIcon.IsVisible = true;
                break;
            case DialogType.Warning:
                WarningIcon.IsVisible = true;
                break;
            case DialogType.Information:
                InfoIcon.IsVisible = true;
                break;
            case DialogType.Notice:
                NoticeIcon.IsVisible = true;
                break;
        }

        if (isConfirmOnly)
            CancelButton.IsVisible = false;
    }

    private void ConfirmButton_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void DialogCloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
