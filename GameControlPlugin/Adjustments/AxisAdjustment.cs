namespace Loupedeck.GameControlPlugin.Adjustments
{
    using System;
    using System.Reflection;

    using Helpers;

    public abstract class AxisAdjustment : PluginDynamicAdjustment
    {
        internal AxisAdjustment(string displayName, string description, string groupName)
            : base(displayName, description, groupName, false)
        {
            DisplayName = $"{displayName} (Rotary Adjustment)";
            GroupName = "Not used";
            MakeProfileAction("text;Enter the adjustment speed (Normal, Fast, Slow, [1-1000]) and any options:");
        }

        protected override void ApplyAdjustment(string actionParameter, int ticks)
        {
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

            if (ticks < int.MaxValue) // We just want to refresh the current value. This is because it was changed indirectly.
            {
                DoAdjustment(GameControlPlugin.GetCommandInfo(actionParameter), actionParameter, ticks);
            }

            AdjustmentValueChanged(actionParameter);
            ActionImageChanged(actionParameter);
        }

        protected abstract void DoAdjustment(CommandInfoType commandInfo, string actionParameter, int ticks);

        protected override void RunCommand(string actionParameter)
        {
            var joystick = JoystickManager.GetJoystick(actionParameter);
            var adjustmentInfo = GetAdjustmentInfo(actionParameter, joystick);

            adjustmentInfo.StickValue = joystick.MaxValue / 2;
            adjustmentInfo.SetStickValue(joystick);
            
            ActionImageChanged(actionParameter);
        }

        protected override string GetAdjustmentValue(string actionParameter)
        {
            var commandInfo = GameControlPlugin.GetCommandInfo(actionParameter);

            if (!commandInfo.DrawNumbers)
                return string.Empty;

            var joystick = JoystickManager.GetJoystick(actionParameter);
            var adjustmentInfo = GetAdjustmentInfo(actionParameter, joystick);

            // MinValue on initialisation
            if (adjustmentInfo.StickValue == int.MinValue)
            {
                adjustmentInfo.StickValue = JoystickManager.GetAxisDefaultValue(actionParameter) ?? joystick.MaxValue / 2;
                
                AdjustmentCache.AddEntry(GetType().FullName, adjustmentInfo.StickId, adjustmentInfo.StickAxis, actionParameter);
            }
            
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

            return ((int)((adjustmentInfo.StickValue / (double)joystick.MaxValue * 2.0 - 1.0) * 100.0)) + "%";
        }

        internal static AdjustmentInfo GetAdjustmentInfo(string actionParameter, Joystick joystick)
        {
            var adjustmentInfo = new AdjustmentInfo();
            
            foreach(var entry in actionParameter.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var values = entry.Split('=', StringSplitOptions.RemoveEmptyEntries);

                if (values.Length < 2)
                    continue;

                switch (values[0].Trim().ToLower())
                {
                    case "axis":
                        adjustmentInfo.SetStickAxis(joystick, values[1].Trim().ToUpper());
                        break;
                    case "upbutton":
                        adjustmentInfo.UpButtonId = int.Parse(values[1].Trim());
                        break;
                    case "downbutton":
                        adjustmentInfo.DownButtonId = int.Parse(values[1].Trim());
                        break;
                }
            }
            
            return adjustmentInfo;
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
            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(commandInfo.RotaryPath));
            if (commandInfo.Label != "")
            {
                bitmapBuilder.FillRectangle(0, (int)(0.6 * (commandInfo.LabelPos + 2)) - commandInfo.LabelSize / 2, 50, commandInfo.LabelSize, commandInfo.LabelBackgroundColor);
                bitmapBuilder.DrawText(commandInfo.Label ?? "", 0, (int)(0.6 * (commandInfo.LabelPos + 2)) - 14, 50, commandInfo.LabelSize + 6, commandInfo.LabelColor, commandInfo.LabelSize, 18);
            }

            return bitmapBuilder.ToImage();
        }
    }

    public class AdjustmentInfo
    {
        private PropertyInfo _stickAxisProperty;
        private HID_USAGES _hidUsages;

        public string StickAxis { get; private set; }
        public int StickValue { get; set; }
        public int? UpButtonId { get; set; }
        public int? DownButtonId { get; set; }
        public uint StickId { get; private set; }

        public void SetStickAxis(Joystick joystick, string stickAxis)
        {
            StickAxis = stickAxis;
            StickId = joystick.Id;

            _stickAxisProperty = joystick.GetType().GetProperty(stickAxis);

            if (!Enum.TryParse($"HID_USAGE_{stickAxis}", out _hidUsages))
                throw new Exception($"The stick axis '{stickAxis}' is invalid. Must set 'axis=<{GameControlPlugin.AxisNames}>'.");

            StickValue = (int?)_stickAxisProperty?.GetValue(joystick) ?? 0;
        }

        internal void SetStickValue(Joystick joystick)
        {
            _stickAxisProperty?.SetValue(joystick, StickValue);

            joystick.SetAxis(StickValue, _hidUsages);
        }
    }
}