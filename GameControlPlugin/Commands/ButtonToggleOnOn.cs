namespace Loupedeck.GameControlPlugin.Commands
{
    using System.Threading.Tasks;

    public class ButtonToggleOnOn() : ButtonCommand("Toggles (On-On)", "text;Enter the first dx button in the toggle (1-127) and any options:")
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
            GameControlPlugin.Buttons[commandInfo.Value + 1] = !GameControlPlugin.Buttons[commandInfo.Value];
            
            joystick.SetBtn(GameControlPlugin.Buttons[commandInfo.Value], (uint)commandInfo.Value);
            joystick.SetBtn(GameControlPlugin.Buttons[commandInfo.Value + 1], (uint)(commandInfo.Value + 1));
            
            if (commandInfo.DXSendType == 0)
            {
                if (GameControlPlugin.Buttons[commandInfo.Value])
                    Task.Delay(JoystickManager.ButtonPressDelay).ContinueWith(t => joystick.SetBtn(false, (uint)commandInfo.Value));
                else
                    Task.Delay(JoystickManager.ButtonPressDelay).ContinueWith(t => joystick.SetBtn(false, (uint)(commandInfo.Value + 1)));
            }

            ActionImageChanged(actionParameter);
        }

        protected override BitmapImage GetCommandImage(
            string actionParameter,
            PluginImageSize imageSize)
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
                bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(commandInfo.DrawToggleIndicators ? GameControlPlugin.ToggleOnOnUpResourcePath : GameControlPlugin.ToggleUpResourcePath));
            else
                bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(commandInfo.DrawToggleIndicators ? GameControlPlugin.ToggleOnOnDownResourcePath : GameControlPlugin.ToggleDownResourcePath));
            if (commandInfo.DrawNumbers)
                bitmapBuilder.DrawText($"{commandInfo.Value}\n \n{commandInfo.Value + 1}", -3, -5, 20, 80, fontSize: 10, lineHeight: 18);
            if (commandInfo.Label != "")
            {
                bitmapBuilder.FillRectangle(0, commandInfo.LabelPos - commandInfo.LabelSize / 2, 80, commandInfo.LabelSize, commandInfo.LabelBackgroundColor);
                bitmapBuilder.DrawText(commandInfo.Label ?? "", 0, commandInfo.LabelPos - 14, 80, commandInfo.LabelSize + 6, commandInfo.LabelColor, commandInfo.LabelSize, 18);
            }

            return bitmapBuilder.ToImage();
        }
    }
}