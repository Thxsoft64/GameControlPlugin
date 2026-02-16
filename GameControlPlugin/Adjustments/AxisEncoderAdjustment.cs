namespace Loupedeck.GameControlPlugin.Adjustments
{
    public class AxisEncoderAdjustment() : AxisAdjustment("Encoder", $"Adjusts a axis using an encoder ({GameControlPlugin.AxisNames})", "Named Axis Adjustment")
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
        }
    }
}