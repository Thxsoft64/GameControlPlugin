namespace Loupedeck.GameControlPlugin.Commands
{
    public class Pov1Press() : PovCommand("POV 1 Press", "text;Enter the direction to press (Up, Down, Left, Right) and any options:")
    {
        protected override void RunCommand(string actionParameter)
        {
            DoCommand(1U, actionParameter);
        }
    }
}