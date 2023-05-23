using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Downloader;
using ICSharpCode.SharpZipLib.Zip;
using LibGit2Sharp;
using Newtonsoft.Json.Linq;

namespace Chief_Reloaded.Views;

public partial class WoolCInstallPage : UserControl
{
    private bool _isFastInstall;
    private bool _isMultiWoolC;
    private List<string> _woolangCompilers = new();

    public WoolCInstallPage()
    {
        InitializeComponent();
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (await Utils.CheckWoolangInstallStatus())
            {
                Startup.IsVisible = true;
                _woolangCompilers = await Utils.FindWoolangInstallationPath();
                if (_woolangCompilers.Count > 1)
                {
                    WoolangComboBox.ItemsSource = _woolangCompilers;
                    WoolangComboBox.SelectedItem = _woolangCompilers.FirstOrDefault();
                    _isMultiWoolC = true;
                }
            }
            else
            {
                Crumbs.IsVisible = true;
                ModeText.Text = "安装";
                InstallModeSelectionPanel.IsVisible = true;
            }
        });
    }

    private void ReturnIndexButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Controls.PageSwitch(this, new MainPage());
    }

    private async void ReturnLastButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Startup.IsVisible)
        {
            ReturnIndexButton_OnClick(sender, e);
        }
        else if (InstallModeSelectionPanel.IsVisible)
        {
            if (await Utils.CheckWoolangInstallStatus())
            {
                Crumbs.IsVisible = false;
                Controls.SwitchControlVisibility(InstallModeSelectionPanel, Startup);
            }
            else
            {
                ReturnIndexButton_OnClick(sender, e);
            }
        }
        else if (FreshInstallPanel.IsVisible)
        {
            Controls.SwitchControlVisibility(FreshInstallPanel, InstallModeSelectionPanel);
        }
        else if (MultiWoolCPanel.IsVisible)
        {
            Crumbs.IsVisible = false;
            Controls.SwitchControlVisibility(MultiWoolCPanel, Startup);
        }
    }

    private void InstallWoolangC_OnClick(object? sender, RoutedEventArgs e)
    {
        Crumbs.IsVisible = true;
        ModeText.Text = "安装";
        Controls.SwitchControlVisibility(Startup, InstallModeSelectionPanel);
    }

    private async void UpdateWoolangC_OnClick(object? sender, RoutedEventArgs e)
    {
        Crumbs.IsVisible = true;
        ModeText.Text = "更新";
        if (_isMultiWoolC)
        {
            Controls.SwitchControlVisibility(Startup, MultiWoolCPanel);
        }
        else
        {
            Controls.SwitchControlVisibility(Startup, ProcessingPanel);
            InstallationPathBox.Text = _woolangCompilers[0];
            if (await FastInstallWoolangCompiler())
            {
                ReturnIndexButton.IsEnabled = true;
                InstallingTitle.Text = "安装完成";
                InstallingSubTitle.IsVisible = false;
                Controls.SwitchControlVisibility(InstallingTip, InstallationCompletedButton);
            }
            else
            {
                ReturnIndexButton.IsEnabled = true;
                InstallingTitle.Text = "安装失败";
                InstallingSubTitle.IsVisible = false;
                Controls.SwitchControlVisibility(InstallingTip, InstallationCompletedButton);
            }
        }
    }

    private async void InstallContinueButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(InstallationPathBox.Text) || !Directory.Exists(InstallationPathBox.Text))
        {
            var dialog = new Dialog();
            dialog.InitDialog(Dialog.DialogType.Error, "无效的安装路径", "请输入或选择一个有效且存在的安装路径", true);
            await dialog.ShowDialog(Controls.GetMainWindow()!);
            return;
        }

        Controls.SwitchControlVisibility(FreshInstallPanel, ProcessingPanel);
        ReturnLastButton.IsEnabled = false;
        ReturnIndexButton.IsEnabled = false;
        bool result;
        if (_isFastInstall)
            result = await Task.Run(FastInstallWoolangCompiler);
        else
            result = await Task.Run(InstallWoolangCompiler);

        if (result)
        {
            ReturnIndexButton.IsEnabled = true;
            InstallingTitle.Text = "安装完成";
            InstallingSubTitle.IsVisible = false;
            Controls.SwitchControlVisibility(InstallingTip, InstallationCompletedButton);
        }
        else
        {
            ReturnIndexButton.IsEnabled = true;
            InstallingTitle.Text = "安装失败";
            InstallingSubTitle.IsVisible = false;
            Controls.SwitchControlVisibility(InstallingTip, InstallationCompletedButton);
        }
    }

    private async void OpenFolderDialogButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var result = await Controls.GetMainWindow()!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = "指定 Woolang 编译器的安装文件夹"
        });
        if (result.Count <= 0) return;
        InstallationPathBox.Text = result[0].Path.AbsolutePath;
    }

    private async Task<bool> FastInstallWoolangCompiler()
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64 &&
            RuntimeInformation.ProcessArchitecture != Architecture.X86)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new Dialog();
                dialog.InitDialog(Dialog.DialogType.Warning, "需要编译安装",
                    "Woolang 尚未提供此平台的预构建可执行程序，因此\n" +
                    "Chief 将尝试为您进行编译安装", true);
                await dialog.ShowDialog(Controls.GetMainWindow()!);
            });
            return await InstallWoolangCompiler();
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            InstallingTitle.Text = "正在获取 Woolang 构建信息...";
            InstallingSubTitle.Text = "这不会需要很长时间";
        });
        var client = new HttpClient();
        // Read-Only Access Token
        client.DefaultRequestHeaders.Add("Authorization", @"Bearer 7gz8QHnPyB_9HWtyjdfP");
        var response = await client.GetAsync(new Uri("https://git.cinogama.net/api/v4/projects/68/jobs"));
        response.EnsureSuccessStatusCode();
        var jobData = JArray.Parse(await response.Content.ReadAsStringAsync());
        var jobs = from item in jobData select (JObject)item;
        var latestBuildId = string.Empty;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            foreach (var item in jobs)
            {
                if (!item["name"]!.ToString().Equals("build_release_win32")) continue;
                latestBuildId = (string)item["id"]!;
                break;
            }
        else
            foreach (var item in jobs)
            {
                if (!item["name"]!.ToString().Equals("build_release")) continue;
                latestBuildId = (string)item["id"]!;
                break;
            }

        var latestBuildUrl =
            $"https://git.cinogama.net/cinogamaproject/woolang/-/jobs/{latestBuildId}/artifacts/download";
        var downloader = new DownloadService(new DownloadConfiguration
        {
            ChunkCount = 8,
            ParallelDownload = true
        });
        var installationPath = string.Empty;
        await Dispatcher.UIThread.InvokeAsync(() => { installationPath = InstallationPathBox.Text!; });
        downloader.DownloadProgressChanged += async (_, args) =>
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                InstallingTitle.Text = "正在下载 Woolang 编译器...";
                InstallingSubTitle.Text = $"已下载 {(int)args.ProgressPercentage}%";
                InstallingProgressBar.Value = args.ProgressPercentage;
            });
        };
        var destinationStream = await downloader.DownloadFileTaskAsync(latestBuildUrl);
        await using var s = new ZipInputStream(destinationStream);

        while (s.GetNextEntry() is { } theEntry)
        {
            var fileName = Path.GetFileName(theEntry.Name);

            if (fileName == string.Empty) continue;
            await using var streamWriter = File.Create(Path.Combine(installationPath, fileName));
            var datBytes = new byte[2048];
            while (true)
            {
                var size = s.Read(datBytes, 0, datBytes.Length);
                if (size > 0)
                    streamWriter.Write(datBytes, 0, size);
                else
                    break;
            }
        }

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
#pragma warning disable CA1416
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
#pragma warning restore CA1416
            {
                var dialogResult = false;
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var dialog = new Dialog();
                    dialog.InitDialog(Dialog.DialogType.Information, "高级环境变量写入",
                        "点击\"确认\"会将 Woolang 的环境目录写入至系统环境变量\n" +
                        "点击\"取消\"则会保持原有设定，写入至用户环境变量。\n" +
                        "如果您希望为这台计算机上所有用户安装，请写入至系统环境变量。");
                    await dialog.ShowDialog(Controls.GetMainWindow()!);
                    dialogResult = dialog.DialogResult;
                });
                if (dialogResult)
                {
                    var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
                    if (path!.Contains(Path.Combine(installationPath) + "/")) return true;
                    if (path.EndsWith(";"))
                        path += Path.Combine(installationPath) + "/";
                    else
                        path += ";" + Path.Combine(installationPath) + "/";
                    Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Machine);
                }
                else
                {
                    var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                    if (path!.Contains(Path.Combine(installationPath) + "/")) return true;
                    if (path.EndsWith(";"))
                        path += Path.Combine(installationPath) + "/";
                    else
                        path += ";" + Path.Combine(installationPath) + "/";
                    Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.User);
                }
            }
            else
            {
                var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                if (path!.Contains(Path.Combine(installationPath) + "/")) return true;
                if (path.EndsWith(";"))
                    path += Path.Combine(installationPath) + "/";
                else
                    path += ";" + Path.Combine(installationPath) + "/";
                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.User);
            }
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new Dialog();
                dialog.InitDialog(Dialog.DialogType.Information, "需要配置环境变量",
                    "Woolang 编译器已经构建完成，如果您需要从其它位置直接访问，\n" +
                    "或使用 Chief 来管理您的后续升级，请配置环境变量。", true);
                await dialog.ShowDialog(Controls.GetMainWindow()!);
            });
        }

        return true;
    }

    private async Task<bool> InstallWoolangCompiler()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            var vswherePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft Visual Studio", "Installer", "vswhere.exe");

            if (File.Exists(vswherePath))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    InstallingProgressBar.IsIndeterminate = true;
                    InstallingTitle.Text = "正在获取系统信息...";
                });
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

                var cmakePath = Path.Combine(vsPath, "CommonExtensions", "Microsoft", "CMake", "CMake", "bin",
                    "cmake.exe");
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
                            dialog.InitDialog(Dialog.DialogType.Error, "未能成功获取 Woolang 编译器源代码",
                                "在尝试拉取 Woolang 编译器源代码时出错，请检查网络连接或稍后再试", true);
                            var window =
                                (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!
                                .MainWindow!;
                            var control = window.Find<TransitioningContentControl>("ContentControl")!;
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

                    var cmake_arguments = string.Empty;
                    if (RuntimeInformation.ProcessArchitecture == Architecture.Arm ||
                        RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ||
                        RuntimeInformation.ProcessArchitecture == Architecture.Armv6)
                    {
                        cmake_arguments =
                            ".. -DWO_SUPPORT_ASMJIT=OFF -DWO_MAKE_OUTPUT_IN_SAME_PATH=ON -DCMAKE_BUILD_TYPE=RELWITHDEBINFO -DBUILD_SHARED_LIBS=ON";
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            InstallingOutputBox.Text += "\nWoolang 编译器正在以 ARM 模式构建，因此 Woolang 的 JIT 功能将被禁用，" +
                                                        "这可能导致轻微的性能损失。\n";
                        });
                    }
                    else
                    {
                        cmake_arguments =
                            ".. -DWO_MAKE_OUTPUT_IN_SAME_PATH=ON -DCMAKE_BUILD_TYPE=RELWITHDEBINFO -DBUILD_SHARED_LIBS=ON";
                    }

                    process = Process.Start(
                        new ProcessStartInfo(cmakePath, cmake_arguments)
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            WorkingDirectory = Path.Combine(repoPath, "build"),
                            CreateNoWindow = true,
                            StandardOutputEncoding = Encoding.Unicode
                        });
                    await process!.WaitForExitAsync();

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
                        Controls.SwitchControlVisibility(InstallingProgressBar, InstallingOutputBox);
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
                    var installationPath = string.Empty;
                    await Dispatcher.UIThread.InvokeAsync(() => { installationPath = InstallationPathBox.Text; });
                    process!.OutputDataReceived += (_, args) =>
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


                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
#pragma warning disable CA1416
                        if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(
                                WindowsBuiltInRole.Administrator))
#pragma warning restore CA1416
                        {
                            var dialogResult = false;
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                var dialog = new Dialog();
                                dialog.InitDialog(Dialog.DialogType.Information, "高级环境变量写入",
                                    "点击\"确认\"会将 Woolang 的环境目录写入至系统环境变量\n" +
                                    "点击\"取消\"则会保持原有设定，写入至用户环境变量。\n" +
                                    "如果您希望为这台计算机上所有用户安装，请写入至系统环境变量。");
                                await dialog.ShowDialog(Controls.GetMainWindow()!);
                                dialogResult = dialog.DialogResult;
                            });
                            if (dialogResult)
                            {
                                var path = Environment.GetEnvironmentVariable("PATH",
                                    EnvironmentVariableTarget.Machine);
                                if (path!.Contains(Path.Combine(installationPath) + "/")) return true;
                                if (path.EndsWith(";"))
                                    path += Path.Combine(installationPath) + "/";
                                else
                                    path += ";" + Path.Combine(installationPath) + "/";
                                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Machine);
                            }
                            else
                            {
                                var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                                if (path!.Contains(Path.Combine(installationPath) + "/")) return true;
                                if (path.EndsWith(";"))
                                    path += Path.Combine(installationPath) + "/";
                                else
                                    path += ";" + Path.Combine(installationPath) + "/";
                                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.User);
                            }
                        }
                        else
                        {
                            var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                            if (path!.Contains(Path.Combine(installationPath) + "/")) return true;
                            if (path.EndsWith(";"))
                                path += Path.Combine(installationPath) + "/";
                            else
                                path += ";" + Path.Combine(installationPath) + "/";
                            Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.User);
                        }
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            var dialog = new Dialog();
                            dialog.InitDialog(Dialog.DialogType.Information, "需要配置环境变量",
                                "Woolang 编译器已经构建完成，但 Chief 不会尝试\n" +
                                "在 Linux 系统上自动添加环境变量，如果您需要从其它位置直接访问，\n" +
                                "或使用 Chief 来管理您的后续升级，请配置环境变量。", true);
                            await dialog.ShowDialog(Controls.GetMainWindow()!);
                        });
                    }

                    return true;
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var dialog = new Dialog();
                dialog.InitDialog(Dialog.DialogType.Error, "未能找到 Visual Studio 构建工具",
                    "构建 Windows 版本的 Woolang 编译器必须使用 Visual Studio 构建工具。\n请安装后重试。",
                    true);
                Controls.PageSwitch(this, new MainPage());
            });
            return false;
        }

        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                InstallingProgressBar.IsIndeterminate = true;
                InstallingTitle.Text = "正在获取系统信息...";
            });
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
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() => { InstallingTitle.Text = "正在获取 Woolang 源码..."; });
                Repository.Clone("https://git.cinogama.net/cinogamaproject/woolang", repoPath);
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

                var cmake_arguments = string.Empty;
                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm ||
                    RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ||
                    RuntimeInformation.ProcessArchitecture == Architecture.Armv6)
                {
                    cmake_arguments =
                        ".. -DWO_SUPPORT_ASMJIT=OFF -DWO_MAKE_OUTPUT_IN_SAME_PATH=ON -DCMAKE_BUILD_TYPE=RELWITHDEBINFO -DBUILD_SHARED_LIBS=ON";
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        InstallingOutputBox.Text += "\nWoolang 编译器正在以 ARM 模式构建，因此 Woolang 的 JIT 功能将被禁用，" +
                                                    "这可能导致轻微的性能损失。\n";
                    });
                }
                else
                {
                    cmake_arguments =
                        ".. -DWO_MAKE_OUTPUT_IN_SAME_PATH=ON -DCMAKE_BUILD_TYPE=RELWITHDEBINFO -DBUILD_SHARED_LIBS=ON";
                }

                var process = Process.Start(
                    new ProcessStartInfo("cmake", cmake_arguments)
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        WorkingDirectory = Path.Combine(repoPath, "build"),
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.Unicode
                    });
                await process!.WaitForExitAsync();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    InstallingTitle.Text = "正在构建...";
                    InstallingOutputBox.Text += "\n";
                });
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Controls.SwitchControlVisibility(InstallingProgressBar, InstallingOutputBox);
                });
                process = Process.Start(
                    new ProcessStartInfo("make",
                        "-j 4")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        WorkingDirectory = Path.Combine(repoPath, "build"),
                        CreateNoWindow = true
                    });
                process!.OutputDataReceived += (_, args) =>
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        InstallingOutputBox.Text += args.Data + "\n";
                        InstallingOutputBox.CaretIndex = int.MaxValue;
                    });
                };
                process.BeginOutputReadLine();
                await process.WaitForExitAsync();
                var installationPath = string.Empty;
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    InstallingTitle.Text = "正在安装...";
                    installationPath = InstallationPathBox.Text;
                });
                File.Copy(Path.Combine(repoPath, "build", "woodriver"),
                    Path.Combine(installationPath, "woodriver"), true);
                File.Copy(Path.Combine(repoPath, "build", "libwoo.so"),
                    Path.Combine(installationPath, "libwoo.so"), true);
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var dialog = new Dialog();
                    dialog.InitDialog(Dialog.DialogType.Information, "需要配置环境变量",
                        "Woolang 编译器已经构建完成，但 Chief 不会尝试\n" +
                        "在 Linux 系统上自动添加环境变量，如果您需要从其它位置直接访问，\n" +
                        "或使用 Chief 来管理您的后续升级，请配置环境变量。", true);
                    await dialog.ShowDialog(
                        (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!
                        .MainWindow!);
                });
            }
            catch (LibGit2SharpException)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var dialog = new Dialog();
                    dialog.InitDialog(Dialog.DialogType.Error, "未能成功获取 Woolang 编译器源代码",
                        "在尝试拉取 Woolang 编译器源代码时出错，请检查网络连接或稍后再试", true);
                    Controls.PageSwitch(this, new WoolCInstallPage());
                });
                return false;
            }
            catch
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var dialog = new Dialog();
                    dialog.InitDialog(Dialog.DialogType.Error, "尝试安装时出错",
                        "在尝试安装 Woolang 编译器时出错", true);
                    Controls.PageSwitch(this, new WoolCInstallPage());
                });
                return false;
            }
        }
        return true;
    }


    private void FastInstallButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _isFastInstall = true;
        Controls.SwitchControlVisibility(InstallModeSelectionPanel, FreshInstallPanel);
    }

    private void BuildInstallButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _isFastInstall = false;
        Controls.SwitchControlVisibility(InstallModeSelectionPanel, FreshInstallPanel);
    }

    private void InstallationCompletedButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ReturnIndexButton_OnClick(sender, e);
    }

    private void WoolangComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        InstallationPathBox.Text =
            WoolangComboBox.SelectedItem != null ? WoolangComboBox.SelectedItem.ToString() : string.Empty;
    }

    private async void UpdateWoolangCButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_isMultiWoolC) Controls.SwitchControlVisibility(MultiWoolCPanel, ProcessingPanel);

        if (await FastInstallWoolangCompiler())
        {
            ReturnIndexButton.IsEnabled = true;
            InstallingTitle.Text = "安装完成";
            InstallingSubTitle.IsVisible = false;
            Controls.SwitchControlVisibility(InstallingTip, InstallationCompletedButton);
        }
        else
        {
            ReturnIndexButton.IsEnabled = true;
            InstallingTitle.Text = "安装失败";
            InstallingSubTitle.IsVisible = false;
            Controls.SwitchControlVisibility(InstallingTip, InstallationCompletedButton);
        }
    }
}