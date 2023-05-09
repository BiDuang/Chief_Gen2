# Chief_Reloaded

<div align=center>
<img alt="Chief_RE Logo" src="https://git.cinogama.net/cinogamaproject/woolang/-/raw/master/image/woolang_logo.png" width="200" />
<h2>提供便捷、快速的 Woolang、BaoZi、JoyEngineECS 安装与版本控制</h2>
<h3>(C) BiDuang 2023</h3>
<img alt="Project Time" src="https://wakatime.com/badge/user/54584642-d17f-456f-9341-29215427f16b/project/70f385fa-9d8a-43ac-aa4d-192f458700fd.svg?style=flat-square"/>
<img alt="License" src="https://img.shields.io/github/license/BiDuang/Chief_Reloaded?color=%2339c5bb&style=flat-square">
</div>

## 功能

跨平台* 以用户界面应用程序提供 Woolang 编译器，BaoZi 包管理器，JoyEngineECS 游戏引擎的安装与版本控制。

相较于 [Chief](https://github.com/BiDuang/Chief)，Chief_Reloaded 提供了更加友好的用户界面，更加便捷的操作方式，更加完善的功能。

**注意: Chief 即将不再受到支持，请尽快迁移至 Chief_Reloaded。**

*: 已在 `Windows 10+` 、 `Linux-x64 20.04` 和 `Linux-ARM64 22.04` 测试。

## 使用方式

### 获取可执行文件

项目仍在不稳定开发阶段 ( Dev ), 暂不提供稳定的构建发行通道。

### 从源码构建

拉取源码到本地，使用 `Visual Studio 2019+` 或 `JetBrains Rider 2023.1+` 还原项目依赖环境后构建。

供参考的构建指令:

```bash
dotnet publish -c Release -r {操作系统环境} --self-contained /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

操作系统环境可填: `win7-x64`, `linux-x64`, `linux-arm64` 等。

## 引用与致谢

在本项目的开发过程中使用或借鉴了以下项目，在此表达感谢 ( 排名不分先后 ):

[Avalonia](https://github.com/AvaloniaUI/Avalonia)

[Downloader](https://github.com/bezzad/Downloader)

[FluentUI Icons](https://github.com/microsoft/fluentui-system-icons)

[Newtonsoft.Json](https://www.newtonsoft.com/json)

[LibGit2Sharp](https://github.com/libgit2/libgit2sharp)

[SharpLibZip](https://github.com/icsharpcode/SharpZipLib)

[Woolang](https://git.cinogama.net/cinogamaproject/woolang),
[BaoZi](https://git.cinogama.net/cinogamaproject/woolangpackages/baozi)
和 [JoyEngineECS](https://git.cinogama.net/cinogamaproject/joyengineecs/joyengineecs)
是 [CinogamaProject](https://git.cinogama.net/cinogamaproject) 的一部分。

作为 Woolang 作者与主要维护者的 [@mr_cino](https://github.com/mrcino) 亲自审查了此项目并提供了开发指导与技术支持。

来自 [InvoluteHell](https://github.com/InvoluteHell) 和 [Friendship Studio](https://github.com/Friendship-Studio)
的开发者们参与了项目的测试并提供了许多有益的反馈。

## 开源协议 (License)

基于 `Apache License 2.0` 开源, 项目 Logo 版权归 [@mr_cino](https://github.com/mrcino) 所有。
