namespace Loupedeck.GameControlPlugin.Commands
{
    using System;

    public abstract class ButtonCommand : PluginDynamicCommand
    {
        protected ButtonCommand(string displayName, string action)
        {
            DisplayName = displayName;
            GroupName = "Not used";
            MakeProfileAction(action);
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            var commandInfo = GameControlPlugin.GetCommandInfo(actionParameter);
            if (GameControlPlugin.PluginError != "")
                Plugin.OnPluginStatusChanged(PluginStatus.Error, GameControlPlugin.PluginError);
            if (GameControlPlugin.PluginWarning != "" && !GameControlPlugin.InWarning)
            {
                Plugin.OnPluginStatusChanged(PluginStatus.Warning, GameControlPlugin.PluginWarning);
                GameControlPlugin.PluginWarning = "";
                GameControlPlugin.InWarning = true;
            }

            if (GameControlPlugin.InWarning && GameControlPlugin.PluginWarningStopwatch.ElapsedMilliseconds > 2000L)
            {
                Plugin.OnPluginStatusChanged(PluginStatus.Normal, null);
                GameControlPlugin.InWarning = false;
            }

            using var bitmapBuilder = new BitmapBuilder(imageSize);
            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(commandInfo.ButtonPath));
            if (commandInfo.DrawNumbers)
                bitmapBuilder.DrawText($"{commandInfo.Value}", fontSize: 20);
            if (commandInfo.Label != "")
            {
                bitmapBuilder.FillRectangle(0, (int)(commandInfo.LabelPos - Math.Round(commandInfo.LabelSize * commandInfo.LabelSize / 28.0)), 80, commandInfo.LabelSize, commandInfo.LabelBackgroundColor);
                bitmapBuilder.DrawText(commandInfo.Label ?? "", 0, commandInfo.LabelPos - 14, 80, commandInfo.LabelSize + 6, commandInfo.LabelColor, commandInfo.LabelSize, 18);
            }

            return bitmapBuilder.ToImage();
        }
    }
}