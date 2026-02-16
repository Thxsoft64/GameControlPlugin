namespace Loupedeck.GameControlPlugin.Commands
{
    public class Pov4Press() : PovCommand("POV 4 Press", "text;Enter the direction to press (Up, Down, Left, Right) and any options:")
    {
        protected override void RunCommand(string actionParameter)
        {
            DoCommand(4U, actionParameter);
        }
    }
}