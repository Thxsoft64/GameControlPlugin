namespace Loupedeck.GameControlPlugin.Commands
{
    using System.Threading.Tasks;

    public class ButtonToggleOnOnOn() : ButtonCommand("Toggles (On-On-On)", "text;Enter the first dx button in the toggle (1-125) and any options:")
    {
        protected override void RunCommand(string actionParameter)
        {
            var CommandInfo = new CommandInfoType();
            CommandInfo = GameControlPlugin.GetCommandInfo(actionParameter);
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

            if (GameControlPlugin.Buttons[CommandInfo.Value])
            {
                GameControlPlugin.Buttons[CommandInfo.Value] = false;
                GameControlPlugin.Buttons[CommandInfo.Value + 1] = true;
                GameControlPlugin.Buttons[CommandInfo.Value + 2] = false;
            }
            else if (GameControlPlugin.Buttons[CommandInfo.Value + 1])
            {
                GameControlPlugin.Buttons[CommandInfo.Value] = false;
                GameControlPlugin.Buttons[CommandInfo.Value + 1] = false;
                GameControlPlugin.Buttons[CommandInfo.Value + 2] = true;
            }
            else
            {
                GameControlPlugin.Buttons[CommandInfo.Value] = true;
                GameControlPlugin.Buttons[CommandInfo.Value + 1] = false;
                GameControlPlugin.Buttons[CommandInfo.Value + 2] = false;
            }

            var joystick = JoystickManager.GetJoystick(actionParameter);

            joystick.SetBtn(GameControlPlugin.Buttons[CommandInfo.Value], (uint)CommandInfo.Value);
            joystick.SetBtn(GameControlPlugin.Buttons[CommandInfo.Value + 1], (uint)(CommandInfo.Value + 1));
            joystick.SetBtn(GameControlPlugin.Buttons[CommandInfo.Value + 2], (uint)(CommandInfo.Value + 2));
            
            if (CommandInfo.DXSendType == 0)
            {
                if (GameControlPlugin.Buttons[CommandInfo.Value])
                    Task.Delay(JoystickManager.ButtonPressDelay).ContinueWith(t => joystick.SetBtn(false, (uint)CommandInfo.Value));
                else if (GameControlPlugin.Buttons[CommandInfo.Value + 1])
                    Task.Delay(JoystickManager.ButtonPressDelay).ContinueWith(t => joystick.SetBtn(false, (uint)(CommandInfo.Value + 1)));
                else
                    Task.Delay(JoystickManager.ButtonPressDelay).ContinueWith(t => joystick.SetBtn(false, (uint)(CommandInfo.Value + 2)));
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
                bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(commandInfo.DrawToggleIndicators ? GameControlPlugin.ToggleOnOnOnUpResourcePath : GameControlPlugin.ToggleUpResourcePath));
            else if (GameControlPlugin.Buttons[commandInfo.Value + 1])
                bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(commandInfo.DrawToggleIndicators ? GameControlPlugin.ToggleOnOnOnMiddleResourcePath : GameControlPlugin.ToggleMiddleResourcePath));
            else
                bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(commandInfo.DrawToggleIndicators ? GameControlPlugin.ToggleOnOnOnDownResourcePath : GameControlPlugin.ToggleDownResourcePath));
            if (commandInfo.DrawNumbers)
                bitmapBuilder.DrawText($"{commandInfo.Value}\n{commandInfo.Value + 1}\n{commandInfo.Value + 2}", -3, -5, 20, 80, fontSize: 10, lineHeight: 18);
            if (commandInfo.Label != "")
            {
                bitmapBuilder.FillRectangle(0, commandInfo.LabelPos - commandInfo.LabelSize / 2, 80, commandInfo.LabelSize, commandInfo.LabelBackgroundColor);
                bitmapBuilder.DrawText(commandInfo.Label ?? "", 0, commandInfo.LabelPos - 14, 80, commandInfo.LabelSize + 6, commandInfo.LabelColor, commandInfo.LabelSize, 18);
            }

            return bitmapBuilder.ToImage();
        }
    }
}