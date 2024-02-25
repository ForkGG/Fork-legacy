using System.Diagnostics;

namespace Fork.logic.Utils;

/// <summary>
///     The ForkUtils class provides general utility methods
/// </summary>
public static class ForkUtils
{
    public static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }
}