namespace Loupedeck.GameControlPlugin.Helpers;
// A helper class that enables logging from the plugin code.

internal static class PluginLog
{
    private static PluginLogFile _pluginLogFile;

    public static void Init(PluginLogFile pluginLogFile)
    {
        pluginLogFile.CheckNullArgument(nameof(pluginLogFile));
        _pluginLogFile = pluginLogFile;
    }

    public static void Verbose(string text) => _pluginLogFile?.Verbose(text);

    public static void Verbose(Exception ex, string text) => _pluginLogFile?.Verbose(ex, text);

    public static void Info(string text) => _pluginLogFile?.Info(text);

    public static void Info(Exception ex, string text) => _pluginLogFile?.Info(ex, text);

    public static void Warning(string text) => _pluginLogFile?.Warning(text);

    public static void Warning(Exception ex, string text) => _pluginLogFile?.Warning(ex, text);

    public static void Error(string text) => _pluginLogFile?.Error(text);

    public static void Error(Exception ex, string text) => _pluginLogFile?.Error(ex, text);
}