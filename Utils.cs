using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chief_Reloaded;

public static partial class Utils
{
    public static async Task<List<string>> FindWoolangInstallationPath()
    {
        var result = new List<string>();
        try
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "where.exe",
                    Arguments = "woodriver.exe",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                result = (await process!.StandardOutput.ReadToEndAsync()).Split("\r\n").ToList();
                result.RemoveAll(x => !x.EndsWith("woodriver.exe"));
                for (var i = 0; i < result.Count; i++)
                {
                    var path = Path.GetDirectoryName(result[i]);
                    if (!string.IsNullOrEmpty(path))
                        result[i] = path;
                }
            }
            else
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "whereis",
                    Arguments = "woodriver",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                result = (await process!.StandardOutput.ReadToEndAsync()).Replace("woodriver: ", "").Split("\n")
                    .ToList();
                result.RemoveAll(x => !x.EndsWith("woodriver"));
                foreach (var item in result)
                {
                    var path = Path.GetDirectoryName(item);
                    if (!string.IsNullOrEmpty(path))
                        result[result.IndexOf(item)] = path;
                }
            }
        }
        catch
        {
            //ignored
        }

        return result;
    }

    public static async Task<bool> CheckWoolangInstallStatus()
    {
        var output = string.Empty;
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "woodriver.exe",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            output = await process!.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            output = RemoveColorCharsRegex().Replace(output, "");
        }
        catch
        {
            // ignored
        }

        return output.StartsWith("Woolang ");
    }

    [GeneratedRegex("\\x1B\\[[0-9;]*[mK]")]
    private static partial Regex RemoveColorCharsRegex();
}