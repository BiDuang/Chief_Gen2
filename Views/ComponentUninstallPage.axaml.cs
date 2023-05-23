using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Avalonia.Controls;
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
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                RemoveEnvPathCheckBox.IsEnabled = false;
                RemoveEnvPathTips.Text = "此功能在非 Windows 系统上不可用，请手动移除环境变量";
            }

            if (!await Utils.CheckWoolangInstallStatus())
            {
                UninstallPanel.IsVisible = false;
                EmptyPanel.IsVisible = true;
            }
        });
    }

    private void ReturnIndexButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Controls.PageSwitch(this, new MainPage());
    }

    private async void UninstallWoolangCButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var woolangPathList = await Utils.FindWoolangInstallationPath();
        if (woolangPathList.Count == 0)
        {
            var dialog = new Dialog();
            dialog.InitDialog(Dialog.DialogType.Error, "尝试卸载时出错", "未能在此设备上找到已安装的 Woolang 编译器", true);
            await dialog.ShowDialog(Controls.GetMainWindow()!);
            return;
        }

        UninstallTitle.Text = "卸载 Woolang 编译器";
        UninstallPathComboBox.ItemsSource = woolangPathList;
        UninstallPathComboBox.SelectedItem = woolangPathList.FirstOrDefault();
        Controls.SwitchControlVisibility(UninstallPanel, UninstallSelectionPanel);
    }

    private void ReturnUninstallMenuButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Controls.SwitchControlVisibility(UninstallSelectionPanel, UninstallPanel);
    }


    private async void ConfirmUninstallButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new Dialog();
        if (UninstallPathComboBox.SelectedItem == null)
        {
            dialog.InitDialog(Dialog.DialogType.Warning, "尝试卸载时出错", "必须指定一个有效的卸载目标。", true);
            await dialog.ShowDialog(Controls.GetMainWindow()!);
            return;
        }

        var path = UninstallPathComboBox.SelectedItem.ToString()!;

        dialog.InitDialog(Dialog.DialogType.Warning, UninstallTitle.Text!, "您确定要卸载此目标吗？");
        await dialog.ShowDialog(Controls.GetMainWindow()!);
        if (!dialog.DialogResult) return;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            if (File.Exists(Path.Combine(path, "woodriver.exe")))
                File.Delete(Path.Combine(path, "woodriver.exe"));
            if (File.Exists(Path.Combine(path, "libwoo.dll")))
                File.Delete(Path.Combine(path, "libwoo.dll"));
            if (RemoveEnvPathCheckBox.IsChecked != true) return;

#pragma warning disable CA1416
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
#pragma warning restore CA1416
            {
                var pathDialog = new Dialog();
                pathDialog.InitDialog(Dialog.DialogType.Information, "高级环境变量卸载", "您要删除机器变量还是用户变量？\n" +
                    "点击\"是\"删除机器变量，点击\"否\"删除用户变量。");
                await pathDialog.ShowDialog(Controls.GetMainWindow()!);
                if (pathDialog.DialogResult)
                {
                    var envPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
                    if (envPath == null) return;
                    var pathList = envPath.Split(';').ToList();
                    pathList.Remove(path);
                    Environment.SetEnvironmentVariable("Path", string.Join(';', pathList),
                        EnvironmentVariableTarget.Machine);
                }
                else
                {
                    var envPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
                    if (envPath == null) return;
                    var pathList = envPath.Split(';').ToList();
                    pathList.Remove(path);
                    Environment.SetEnvironmentVariable("Path", string.Join(';', pathList),
                        EnvironmentVariableTarget.User);
                }
            }
            else
            {
                var envPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
                if (envPath == null) return;
                var pathList = envPath.Split(';').ToList();
                pathList.Remove(path);
                Environment.SetEnvironmentVariable("Path", string.Join(';', pathList),
                    EnvironmentVariableTarget.User);
            }
        }
        else
        {
            if (File.Exists(Path.Combine(path, "woodriver")))
                File.Delete(Path.Combine(path, "woodriver"));
            if (File.Exists(Path.Combine(path, "libwoo.so")))
                File.Delete(Path.Combine(path, "libwoo.so"));
        }

        var noticeDialog = new Dialog();
        noticeDialog.InitDialog(Dialog.DialogType.Information, "卸载完成", "Chief 已成功卸载了指定目标", true);
        await noticeDialog.ShowDialog(Controls.GetMainWindow()!);
        Controls.SwitchControlVisibility(UninstallSelectionPanel, UninstallPanel);
    }
}