using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibGit2Sharp;

namespace Chief_Reloaded.Views.Pages;

public partial class WoolCInstallPage : UserControl
{
    private bool IsFastInstall;

    public WoolCInstallPage()
    {
        InitializeComponent();
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (await CheckWoolangInstallStatus())
                Startup.IsVisible = true;
            else
                FreshInstallPanel.IsVisible = true;
        });
    }

    private static void SwitchControlVisibility(Visual from, Visual to)
    {
        var transition = new CrossFade(TimeSpan.FromMilliseconds(500));
        transition.Start(from, to, new CancellationToken());
    }

    private void ReturnIndexButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow;
        var control = window.Find<TransitioningContentControl>("ContentControl");
        control.Content = new MainPage();
    }

    private async void ReturnLastButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Startup.IsVisible)
        {
            ReturnIndexButton_OnClick(sender, e);
        }
        else if (InstallModeSelectionPanel.IsVisible)
        {
            if (await CheckWoolangInstallStatus())
            {
                Crumbs.IsVisible = false;
                SwitchControlVisibility(InstallModeSelectionPanel, Startup);
            }
            else
            {
                ReturnIndexButton_OnClick(sender, e);
            }
        }
        else if (FreshInstallPanel.IsVisible)
        {
            SwitchControlVisibility(FreshInstallPanel, InstallModeSelectionPanel);
        }
        else if (MultiWoolCPanel.IsVisible)
        {
            Crumbs.IsVisible = false;
            SwitchControlVisibility(MultiWoolCPanel, Startup);
        }
    }

    private void InstallWoolangC_OnClick(object? sender, RoutedEventArgs e)
    {
        Crumbs.IsVisible = true;
        ModeText.Text = "安装";
        SwitchControlVisibility(Startup, InstallModeSelectionPanel);
    }

    private void UpdateWoolangC_OnClick(object? sender, RoutedEventArgs e)
    {
        Crumbs.IsVisible = true;
        ModeText.Text = "更新";
        throw new NotImplementedException();
    }

    private async void InstallContinueButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(InstallationPathBox.Text) || !Directory.Exists(InstallationPathBox.Text))
        {
            var dialog = new Dialog();
            dialog.InitDialog(Dialog.DialogType.Error, "无效的安装路径", "请输入或选择一个有效且存在的安装路径", true);
            await dialog.ShowDialog(
                (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!
                .MainWindow);
            return;
        }

        SwitchControlVisibility(FreshInstallPanel, ProcessingPanel);
        ReturnLastButton.IsEnabled = false;
        ReturnIndexButton.IsEnabled = false;
        var result = await Task.Run(InstallWoolangCompiler);
        ReturnIndexButton.IsEnabled = true;
        InstallingTitle.Text = "安装完成";
        InstallingSubTitle.IsVisible = false;
        SwitchControlVisibility(InstallingTip, InstallationCompletedButton);
    }

    private void OpenFolderDialogButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "请选择 Woolang 编译器的安装路径",
            Directory = Environment.CurrentDirectory
        };
        var result = dialog.ShowAsync(
            (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!
            .MainWindow).Result;
        if (string.IsNullOrEmpty(result)) return;

        InstallationPathBox.Text = result;
    }

    private static async Task<bool> CheckWoolangInstallStatus()
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "woodriver.exe",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        });
        var output = await process!.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        var regex = new Regex(@"\x1B\[[0-9;]*[mK]");
        output = regex.Replace(output, "");
        return output.StartsWith("Woolang ");
    }

    private async Task<bool> InstallWoolangCompiler()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            var vswherePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft Visual Studio", "Installer", "vswhere.exe");

            if (File.Exists(vswherePath))
            {
                if (await BuildWoolangCByVs(vswherePath, InstallationPathBox.Text))
                    return true;
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var dialog = new Dialog();
                    dialog.InitDialog(Dialog.DialogType.Error, "未能找到 Visual Studio 构建工具",
                        "构建 Windows 版本的 Woolang 编译器必须使用 Visual Studio 的 MSBuild 构建工具。\n请安装后重试。",
                        true);
                    var window = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!
                        .MainWindow;
                    var control = window.Find<TransitioningContentControl>("ContentControl");
                    dialog.ShowDialog(window);
                    control.Content = new WoolCInstallPage();
                });
                return false;
            }
        }

        return false;
    }

    private async Task<bool> BuildWoolangCByVs(string vswherePath, string installationPath)
    {
        await Dispatcher.UIThread.InvokeAsync(() => { InstallingTitle.Text = "正在获取系统信息..."; });
        var process = Process.Start(
            new ProcessStartInfo(vswherePath, "-latest -property productPath")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
        var vsPath = (await process!.StandardOutput.ReadToEndAsync()).Trim();
        await process.WaitForExitAsync();
        if (vsPath == string.Empty)
            return false;
        vsPath = Path.GetDirectoryName(vsPath)!;

        var cachePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Chief",
                "RepositoryCache");

        var repoPath = Path.Combine(cachePath, "WoolangC");
        if (Directory.Exists(repoPath))
        {
            var directory = new DirectoryInfo(repoPath) { Attributes = FileAttributes.Normal };
            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
                info.Attributes = FileAttributes.Normal;

            Directory.Delete(repoPath, true);
        }

        Directory.CreateDirectory(repoPath);

        var cmakePath = Path.Combine(vsPath, "CommonExtensions", "Microsoft", "CMake", "CMake", "bin", "cmake.exe");
        if (File.Exists(cmakePath))
        {
            await Dispatcher.UIThread.InvokeAsync(() => { InstallingTitle.Text = "正在获取 Woolang 源码..."; });
            try
            {
                Repository.Clone("https://git.cinogama.net/cinogamaproject/woolang", repoPath);
            }
            catch (LibGit2SharpException)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var dialog = new Dialog();
                    dialog.InitDialog(Dialog.DialogType.Error, "未能找到 Visual Studio 构建工具",
                        "构建 Windows 版本的 Woolang 编译器必须使用 Visual Studio 的 MSBuild 构建工具。\n请安装后重试。",
                        true);
                    var window = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!
                        .MainWindow;
                    var control = window.Find<TransitioningContentControl>("ContentControl");
                    dialog.ShowDialog(window);
                    control.Content = new WoolCInstallPage();
                });
                return false;
            }

            var repo = new Repository(repoPath);
            var branch = repo.Branches["remotes/origin/release"];
            if (branch == null) return false;

            var currentBranch = Commands.Checkout(repo, branch);
            await Dispatcher.UIThread.InvokeAsync(() => { InstallingTitle.Text = "正在更新依赖模块..."; });
            foreach (var submodule in repo.Submodules)
            {
                var options = new SubmoduleUpdateOptions
                {
                    Init = true
                };
                repo.Submodules.Update(submodule.Name, options);
            }

            await Dispatcher.UIThread.InvokeAsync(() => { InstallingTitle.Text = "正在预编译..."; });
            var woInfoPath = Path.Combine(repoPath, "src", "wo_info.hpp");
            await File.WriteAllTextAsync(woInfoPath, "\"" + currentBranch.Tip.Sha + "\"");
            Directory.CreateDirectory(Path.Combine(repoPath, "build"));
            File.Delete(Path.Combine(repoPath, "build", "CMakeCache.txt"));

            process = Process.Start(
                new ProcessStartInfo(cmakePath,
                    ".. -DWO_MAKE_OUTPUT_IN_SAME_PATH=ON -DCMAKE_BUILD_TYPE=RELWITHDEBINFO -DBUILD_SHARED_LIBS=ON")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Path.Combine(repoPath, "build"),
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.Unicode
                });
            await process!.WaitForExitAsync();

            await Dispatcher.UIThread.InvokeAsync(() => { InstallingTitle.Text = string.Empty; });
            process = Process.Start(
                new ProcessStartInfo(vswherePath, "-latest -property installationPath")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });
            var vsInstallationPath = (await process!.StandardOutput.ReadToEndAsync()).Trim();
            await process.WaitForExitAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                InstallingTitle.Text = "正在构建...";
                InstallingOutputBox.Text += "\n";
            });
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SwitchControlVisibility(InstallingProgressBar, InstallingOutputBox);
            });
            var msbuildPath = Path.Combine(vsInstallationPath, "MSBuild", "Current", "Bin", "MSBuild.exe");
            process = Process.Start(
                new ProcessStartInfo(msbuildPath,
                    " ./driver/woodriver.vcxproj /p:Configuration=Release -maxCpuCount -m")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Path.Combine(repoPath, "build"),
                    CreateNoWindow = true
                });
            process!.OutputDataReceived += (sender, args) =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    InstallingOutputBox.Text += args.Data + "\n";
                    InstallingOutputBox.CaretIndex = int.MaxValue;
                });
            };
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();
            await Dispatcher.UIThread.InvokeAsync(() => { InstallingTitle.Text = "正在安装..."; });
            File.Copy(Path.Combine(repoPath, "build", "Release", "woodriver.exe"),
                Path.Combine(installationPath, "woodriver.exe"), true);
            File.Copy(Path.Combine(repoPath, "build", "Release", "libwoo.dll"),
                Path.Combine(installationPath, "libwoo.dll"), true);
            return true;
        }

        return false;
    }


    private void FastInstallButton_OnClick(object? sender, RoutedEventArgs e)
    {
        IsFastInstall = true;
        SwitchControlVisibility(InstallModeSelectionPanel, FreshInstallPanel);
    }

    private void BuildInstallButton_OnClick(object? sender, RoutedEventArgs e)
    {
        IsFastInstall = false;
        SwitchControlVisibility(InstallModeSelectionPanel, FreshInstallPanel);
    }

    private void InstallationCompletedButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ReturnIndexButton_OnClick(sender, e);
    }
}