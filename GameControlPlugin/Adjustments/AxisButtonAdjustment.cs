namespace Loupedeck.GameControlPlugin.Adjustments
{
    using Commands;

    public class AxisButtonAdjustment() : AxisAdjustment("Encoder via Button Presses", $"Adjusts an axis using buttons (UpButton=X,DownButton=Y for axis {GameControlPlugin.AxisNames})", "Button Axis Adjustment")
    {
        protected override void DoAdjustment(CommandInfoType commandInfo, string actionParameter, int ticks)
        {
            var joystick = JoystickManager.GetJoystick(actionParameter);
            var adjustmentInfo = GetAdjustmentInfo(actionParameter, joystick);

            adjustmentInfo.StickValue += ticks * commandInfo.Value;

            if (adjustmentInfo.StickValue < 0)
                adjustmentInfo.StickValue = 0;

            if (adjustmentInfo.StickValue > joystick.MaxValue)
                adjustmentInfo.StickValue = joystick.MaxValue;

            adjustmentInfo.SetStickValue(joystick);

            switch (ticks)
            {
                case > 0 when adjustmentInfo.UpButtonId.HasValue:
                    Plugin.ExecuteGenericAction(typeof(ButtonPress).FullName, $"{actionParameter};{adjustmentInfo.UpButtonId}", 1);
                    break;
                case < 0 when adjustmentInfo.DownButtonId.HasValue:
                    Plugin.ExecuteGenericAction(typeof(ButtonPress).FullName, $"{actionParameter};{adjustmentInfo.DownButtonId}", 1);
                    break;
            }
        }
    }
}