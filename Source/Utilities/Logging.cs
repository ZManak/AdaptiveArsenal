using System.Text;

namespace AdaptiveArsenal.Utilities;

internal static class Logging
{
    private static bool _isDevModeEnabled;
    private static bool IsDevModeEnabled => _isDevModeEnabled;

    static Logging()
    {
        UpdateDevModeStatus();
    }

    public static void UpdateDevModeStatus()
    {
        _isDevModeEnabled = AdaptiveArsenal.Settings.ArsenalSettings.Options.DevMode == 0;
    }
    internal static void Log(string message, params object[] parameters)
    {
        if (IsDevModeEnabled)
        {
            Melon<Mod>.Logger.Msg(message, parameters);
        }
    }

    internal static void LogDebug(string message, params object[] parameters)
    {
        if (IsDevModeEnabled)
        {
            Melon<Mod>.Logger.Msg($"[DEBUG] {message}", parameters);
        }
    }

    internal static void LogWarning(string message, params object[] parameters) => Melon<Mod>.Logger.Warning(message, parameters);
    internal static void LogError(string message, params object[] parameters) => Melon<Mod>.Logger.Error(message, parameters);
    internal static void LogException(string message, Exception exception, params object[] parameters)
    {
        StringBuilder sb = new();

        sb.Append("[EXCEPTION]");
        sb.Append(message);
        sb.AppendLine(exception.Message);

        Melon<Mod>.Logger.Error(sb.ToString(), Color.red, parameters);
    }
}