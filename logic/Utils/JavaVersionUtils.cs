﻿using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Fork.Logic.Model;
using Fork.Logic.Persistence;

namespace Fork.Logic.Utils;

public class JavaVersionUtils
{
    private static readonly Regex versionRegex = new(".* version \"([0-9._]*)\"");
    private static readonly string bitPattern = "64-Bit";

    public static JavaVersion GetInstalledJavaVersion(string javaPath = "")
    {
        if (javaPath.Equals(""))
        {
            javaPath = AppSettingsSerializer.Instance.AppSettings.DefaultJavaPath;
        }

        return CheckForPathJava(javaPath);
    }

    private static JavaVersion CheckForPathJava(string javaPath)
    {
        try
        {
            ProcessStartInfo procStartInfo = new(javaPath, "-version ");

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;
            Process proc = new() { StartInfo = procStartInfo };
            proc.Start();
            return InterpretJavaVersionOutput(proc.StandardError.ReadToEnd());
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static JavaVersion InterpretJavaVersionOutput(string output)
    {
        if (output == null)
        {
            return null;
        }

        Match versionMatch = versionRegex.Match(output);
        if (versionMatch.Success)
        {
            JavaVersion result = new() { Version = versionMatch.Groups[1].Value };
            if (TryParseJavaVersion(result.Version.Split(".")[0], out int computedVersion))
            {
                if (computedVersion == 1)
                {
                    TryParseJavaVersion(result.Version.Split(".")[1], out computedVersion);
                }

                result.VersionComputed = computedVersion;
            }

            if (output.Contains(bitPattern))
            {
                result.Is64Bit = true;
            }

            return result;
        }

        return null;
    }

    private static bool TryParseJavaVersion(string versionString, out int version)
    {
        return int.TryParse(versionString, out version) || int.TryParse(versionString.Split(".")[1], out version);
    }
}