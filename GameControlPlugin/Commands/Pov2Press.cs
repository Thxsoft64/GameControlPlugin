namespace Loupedeck.GameControlPlugin.Commands
{
    public class Pov2Press() : PovCommand("POV 2 Press", "text;Enter the direction to press (Up, Down, Left, Right) and any options:")
    {
        protected override void RunCommand(string actionParameter)
        {
            DoCommand(2U, actionParameter);
        }
    }
}