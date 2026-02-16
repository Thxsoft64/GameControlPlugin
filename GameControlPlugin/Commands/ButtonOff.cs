namespace Loupedeck.GameControlPlugin.Commands
{
    public class ButtonOff() : ButtonCommand("Button Off", "text;Enter the dx button to turn off (1-128) and any options:")
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

            joystick.SetBtn(false, (uint)commandInfo.Value);
            
            ActionImageChanged(actionParameter);
        }
    }
}