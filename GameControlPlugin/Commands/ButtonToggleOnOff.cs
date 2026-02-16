namespace Loupedeck.GameControlPlugin.Commands
{
    public class ButtonToggleOnOff() : ButtonCommand("Toggles (On-Off)", "text;Enter the first dx button in the toggle (1-127) and any options:")
    {
        protected override void RunCommand(string actionParameter)
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

            var joystick = JoystickManager.GetJoystick(actionParameter);

            GameControlPlugin.Buttons[commandInfo.Value] = !GameControlPlugin.Buttons[commandInfo.Value];
            
            joystick.SetBtn(GameControlPlugin.Buttons[commandInfo.Value], (uint)commandInfo.Value);
            
            ActionImageChanged(actionParameter);
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
            if (GameControlPlugin.Buttons[commandInfo.Value])
            {
                if (commandInfo.ToggleAsButton)
                    bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(commandInfo.ButtonPath));
                else
                    bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(commandInfo.DrawToggleIndicators ? GameControlPlugin.ToggleOnOffUpResourcePath : GameControlPlugin.ToggleUpResourcePath));
            }
            else if (commandInfo.ToggleAsButton)
                bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(commandInfo.ButtonPath));
            else
                bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(commandInfo.DrawToggleIndicators ? GameControlPlugin.ToggleOnOffDownResourcePath : GameControlPlugin.ToggleDownResourcePath));

            if (commandInfo.DrawNumbers)
            {
                if (commandInfo.ToggleAsButton)
                    bitmapBuilder.DrawText($"{commandInfo.Value}", fontSize: 20);
                else
                    bitmapBuilder.DrawText($"{commandInfo.Value}", -3, -5, 20, 39, fontSize: 10, lineHeight: 18);
            }

            if (commandInfo.Label != "")
            {
                bitmapBuilder.FillRectangle(0, commandInfo.LabelPos - commandInfo.LabelSize / 2, 80, commandInfo.LabelSize, commandInfo.LabelBackgroundColor);
                bitmapBuilder.DrawText(commandInfo.Label ?? "", 0, commandInfo.LabelPos - 14, 80, commandInfo.LabelSize + 6, commandInfo.LabelColor, commandInfo.LabelSize, 18);
            }

            if (!GameControlPlugin.Buttons[commandInfo.Value] && commandInfo.ToggleAsButton)
                bitmapBuilder.FillRectangle(0, 0, 80, 80, new BitmapColor(0, 0, 0, 155));
            return bitmapBuilder.ToImage();
        }
    }
}