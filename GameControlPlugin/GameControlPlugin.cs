using Loupedeck.GameControlPlugin.Helpers;

namespace Loupedeck.GameControlPlugin;

using System;
using System.Diagnostics;
using System.IO;

public class GameControlPlugin : Plugin
{
    public const string AxisNames = "X, Y, Z, RX, RY, RZ, SL0, SL1";
    public static readonly bool[] Buttons = new bool[200];
    public static bool DefaultDrawNumbers = true;
    public static bool DefaultDrawToggleIndicators = true;
    public static BitmapColor DefaultLabelBackgroundColor = BitmapColor.Transparent;
    public static BitmapColor DefaultLabelColor = BitmapColor.White;
    public static int DefaultLabelSize = 14;
    public static int DefaultLabelPos = 7;
    public static int DefaultDXSendType;
    public static bool DefaultToggleAsButton;
    public static int DefaultAdjustmentSpeed = 100;
    public static string ToggleOnOffOnUpResourcePath;
    public static string ToggleOnOffOnMiddleResourcePath;
    public static string ToggleOnOffOnDownResourcePath;
    public static string ToggleOnOnOnUpResourcePath;
    public static string ToggleOnOnOnMiddleResourcePath;
    public static string ToggleOnOnOnDownResourcePath;
    public static string ToggleOnOnUpResourcePath;
    public static string ToggleOnOnDownResourcePath;
    public static string ToggleOnOffUpResourcePath;
    public static string ToggleOnOffDownResourcePath;
    public static string ToggleDownResourcePath;
    public static string ToggleUpResourcePath;
    public static string ToggleMiddleResourcePath;
    public static string DefaultButtonPath;
    public static string DefaultRotaryPath;
    public static string PluginError;
    public static string PluginWarning;
    public static Stopwatch PluginWarningStopwatch;
    public static bool InWarning;

    public override bool UsesApplicationApiOnly => true;

    public override bool HasNoApplication => true;

    public GameControlPlugin()
    {
        PluginLog.Init(Log);

        PluginResources.Init(this.Assembly);
    }
        
    public override void Load()
    {
        Info.DisplayName = "GameControl";
        Info.Homepage = "https://loupedeck.com/developer/";
        Info.Icon16x16 = EmbeddedResources.ReadImage("Loupedeck.GameControlPlugin.Resources.Icon16x16.png");
        Info.Icon32x32 = EmbeddedResources.ReadImage("Loupedeck.GameControlPlugin.Resources.Icon32x32.png");
        Info.Icon48x48 = EmbeddedResources.ReadImage("Loupedeck.GameControlPlugin.Resources.Icon48x48.png");
        Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.GameControlPlugin.Resources.Icon256x256.png");
        ToggleOnOffOnUpResourcePath = EmbeddedResources.FindFile("ToggleOnOffOnUpDark.png");
        ToggleOnOffOnMiddleResourcePath = EmbeddedResources.FindFile("ToggleOnOffOnMiddleDark.png");
        ToggleOnOffOnDownResourcePath = EmbeddedResources.FindFile("ToggleOnOffOnDownDark.png");
        ToggleOnOnOnUpResourcePath = EmbeddedResources.FindFile("ToggleOnOnOnUpDark.png");
        ToggleOnOnOnMiddleResourcePath = EmbeddedResources.FindFile("ToggleOnOnOnMiddleDark.png");
        ToggleOnOnOnDownResourcePath = EmbeddedResources.FindFile("ToggleOnOnOnDownDark.png");
        ToggleOnOnUpResourcePath = EmbeddedResources.FindFile("ToggleOnOnUpDark.png");
        ToggleOnOnDownResourcePath = EmbeddedResources.FindFile("ToggleOnOnDownDark.png");
        ToggleOnOffUpResourcePath = EmbeddedResources.FindFile("ToggleOnOffUpDark.png");
        ToggleOnOffDownResourcePath = EmbeddedResources.FindFile("ToggleOnOffDownDark.png");
        ToggleUpResourcePath = EmbeddedResources.FindFile("ToggleUpDark.png");
        ToggleDownResourcePath = EmbeddedResources.FindFile("ToggleDownDark.png");
        ToggleMiddleResourcePath = EmbeddedResources.FindFile("ToggleMiddleDark.png");
        DefaultButtonPath = EmbeddedResources.FindFile("BlackSquareButton.png");
        DefaultRotaryPath = EmbeddedResources.FindFile("GrayRotary.png");
        PluginError = "";
        PluginWarning = "";
        PluginWarningStopwatch = new Stopwatch();
        InWarning = false;

        try
        {
            JoystickManager.Initialise(this);

            var pluginDataDirectory = GetPluginDataDirectory();
            if (IoHelpers.EnsureDirectoryExists(pluginDataDirectory))
            {
                var path = Path.Combine(pluginDataDirectory, "Settings.txt");
                if (File.Exists(path))
                {
                    using var streamReader = new StreamReader(path);
                    for (var text = streamReader.ReadLine(); text != null; text = streamReader.ReadLine())
                    {
                        var str = text.Trim();
                        if (str.Length > 0 && str[0] != '#')
                        {
                            var strArray = text.Split("=");
                            var lower1 = strArray[0].Trim().ToLower();
                            if (strArray.Length == 2)
                            {
                                var lower2 = strArray[1].Trim().ToLower();
                                switch (lower1)
                                {
                                    case "adjustmentspeed":
                                    case "as":
                                        switch (lower2)
                                        {
                                            case "f":
                                            case "fast":
                                                DefaultAdjustmentSpeed = 500;
                                                continue;
                                            case "n":
                                            case "norm":
                                            case "normal":
                                                DefaultAdjustmentSpeed = 100;
                                                continue;
                                            case "s":
                                            case "slow":
                                                DefaultAdjustmentSpeed = 25;
                                                continue;
                                            default:
                                                if (!int.TryParse(lower2, out DefaultAdjustmentSpeed))
                                                {
                                                    OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Invalid Default Adjustment Speed (not a number). Fix in settings.txt and restart.");
                                                }

                                                continue;
                                        }
                                    case "bt":
                                    case "buttontype":
                                        switch (lower2)
                                        {
                                            case "black round":
                                            case "br":
                                            case "bround":
                                                DefaultButtonPath = EmbeddedResources.FindFile("BlackRoundButton.png");
                                                continue;
                                            case "ccw":
                                            case "counterclockwise":
                                                DefaultButtonPath = EmbeddedResources.FindFile("BlackSquareCCWButton.png");
                                                continue;
                                            case "clockwise":
                                            case "cw":
                                                DefaultButtonPath = EmbeddedResources.FindFile("BlackSquareCWButton.png");
                                                continue;
                                            case "d":
                                            case "down":
                                                DefaultButtonPath = EmbeddedResources.FindFile("BlackSquareDownButton.png");
                                                continue;
                                            case "gr":
                                            case "gray round":
                                            case "ground":
                                                DefaultButtonPath = EmbeddedResources.FindFile("GrayRoundButton.png");
                                                continue;
                                            case "gray square":
                                            case "gs":
                                            case "gsquare":
                                                DefaultButtonPath = EmbeddedResources.FindFile("GraySquareButton.png");
                                                continue;
                                            case "l":
                                            case "left":
                                                DefaultButtonPath = EmbeddedResources.FindFile("BlackSquareLeftButton.png");
                                                continue;
                                            case "r":
                                            case "right":
                                                DefaultButtonPath = EmbeddedResources.FindFile("BlackSquareRightButton.png");
                                                continue;
                                            case "red round":
                                            case "rr":
                                            case "rround":
                                                DefaultButtonPath = EmbeddedResources.FindFile("RedRoundButton.png");
                                                continue;
                                            case "u":
                                            case "up":
                                                DefaultButtonPath = EmbeddedResources.FindFile("BlackSquareUpButton.png");
                                                continue;
                                            default:
                                                DefaultButtonPath = EmbeddedResources.FindFile("BlackSquareButton.png");
                                                continue;
                                        }
                                    case "dn":
                                    case "drawnumbers":
                                        switch (lower2)
                                        {
                                            case "true":
                                            case "on":
                                            case "yes":
                                                DefaultDrawNumbers = true;
                                                continue;
                                            case "false":
                                            case "off":
                                            case "no":
                                                DefaultDrawNumbers = false;
                                                continue;
                                            default:
                                                continue;
                                        }
                                    case "drawtoggleindicators":
                                    case "dti":
                                        switch (lower2)
                                        {
                                            case "true":
                                            case "on":
                                            case "yes":
                                                DefaultDrawToggleIndicators = true;
                                                continue;
                                            case "false":
                                            case "off":
                                            case "no":
                                                DefaultDrawToggleIndicators = false;
                                                continue;
                                            default:
                                                continue;
                                        }
                                    case "dx":
                                    case "dxsendtype":
                                    case "dxst":
                                        switch (lower2)
                                        {
                                            case "pulse":
                                            case "p":
                                                DefaultDXSendType = 0;
                                                continue;
                                            case "hold":
                                            case "h":
                                                DefaultDXSendType = 1;
                                                continue;
                                            default:
                                                continue;
                                        }
                                    case "labelbackgroundcolor":
                                    case "lb":
                                    case "lbc":
                                        switch (lower2)
                                        {
                                            case "black":
                                                DefaultLabelBackgroundColor = BitmapColor.Black;
                                                continue;
                                            case "blue":
                                                DefaultLabelBackgroundColor = new BitmapColor(50, 50, 200);
                                                continue;
                                            case "gray":
                                                DefaultLabelBackgroundColor = new BitmapColor(128, 128, 128);
                                                continue;
                                            case "green":
                                                DefaultLabelBackgroundColor = new BitmapColor(50, 200, 50);
                                                continue;
                                            case "none":
                                                DefaultLabelBackgroundColor = BitmapColor.Transparent;
                                                continue;
                                            case "purple":
                                                DefaultLabelBackgroundColor = new BitmapColor(150, 50, 200);
                                                continue;
                                            case "red":
                                                DefaultLabelBackgroundColor = new BitmapColor(200, 50, 50);
                                                continue;
                                            default:
                                                if (!uint.TryParse(lower2, out var result2))
                                                {
                                                    OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Invalid Default Label Background Color. Fix in settings.txt and restart.");
                                                    continue;
                                                }

                                                DefaultLabelBackgroundColor = new BitmapColor(result2);
                                                continue;
                                        }
                                    case "labelcolor":
                                    case "lc":
                                        switch (lower2)
                                        {
                                            case "black":
                                                DefaultLabelColor = BitmapColor.Black;
                                                continue;
                                            case "blue":
                                                DefaultLabelColor = new BitmapColor(50, 50, 200);
                                                continue;
                                            case "gray":
                                                DefaultLabelColor = new BitmapColor(128, 128, 128);
                                                continue;
                                            case "green":
                                                DefaultLabelColor = new BitmapColor(50, 200, 50);
                                                continue;
                                            case "purple":
                                                DefaultLabelColor = new BitmapColor(150, 50, 200);
                                                continue;
                                            case "red":
                                                DefaultLabelColor = new BitmapColor(200, 50, 50);
                                                continue;
                                            case "white":
                                                DefaultLabelColor = BitmapColor.White;
                                                continue;
                                            default:
                                                if (!uint.TryParse(lower2, out var result3))
                                                {
                                                    OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Invalid Default Label Color. Fix in settings.txt and restart.");
                                                    continue;
                                                }

                                                DefaultLabelColor = new BitmapColor(result3);
                                                continue;
                                        }
                                    case "labelpos":
                                    case "lp":
                                        switch (lower2)
                                        {
                                            case "top":
                                            case "t":
                                                DefaultLabelPos = 7;
                                                continue;
                                            case "center":
                                            case "c":
                                                DefaultLabelPos = 40;
                                                continue;
                                            case "bottom":
                                            case "b":
                                                DefaultLabelPos = 73;
                                                continue;
                                            default:
                                                if (!int.TryParse(lower2, out DefaultLabelPos))
                                                {
                                                    OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Invalid Default Label Position (must be top, center, bottom, or a number). Fix in settings.txt and restart.");
                                                }

                                                continue;
                                        }
                                    case "labelsize":
                                    case "ls":
                                        var result4 = 14;
                                        if (!int.TryParse(lower2, out result4))
                                        {
                                            OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Invalid Default Label Size (must be a number). Fix in settings.txt and restart.");
                                            continue;
                                        }

                                        DefaultLabelSize = 14;
                                        continue;
                                    case "rotarytype":
                                    case "rt":
                                        if (!(lower2 == "gray"))
                                        {
                                            var num = lower2 == "g" ? 1 : 0;
                                        }

                                        DefaultRotaryPath = EmbeddedResources.FindFile("GrayRotary.png");
                                        continue;
                                    case "tab":
                                    case "toggleasbutton":
                                        switch (lower2)
                                        {
                                            case "true":
                                            case "on":
                                            case "yes":
                                                DefaultToggleAsButton = true;
                                                continue;
                                            case "false":
                                            case "off":
                                            case "no":
                                                DefaultToggleAsButton = false;
                                                continue;
                                            default:
                                                continue;
                                        }
                                    case "vjoy number":
                                        if (int.TryParse(lower2, out var result1))
                                        {
                                            if (result1 > 0 && result1 < 17)
                                            {
                                                JoystickManager.SetDefaultJoystickId((uint)result1);
                                                continue;
                                            }

                                            OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Invalid vJoy ID.  Value must be between 1 and 16.  Please fix in settings.txt and restart");
                                            continue;
                                        }

                                        OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Invalid Default vJoy ID.  A number between 1 and 16 must be set.  Please fix in settings.txt and restart");
                                        continue;
                                    default:
                                        continue;
                                }
                            }

                            OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Invalid settings.txt option format (must be [Option] = [Value]). Fix in settings.txt and restart.");
                        }
                    }
                }
                else
                {
                    using var streamWriter = new StreamWriter(path);
                    streamWriter.WriteLine("vJoy Number = 1");
                    streamWriter.WriteLine("");
                    streamWriter.WriteLine("## Defaults ##");
                    streamWriter.WriteLine("LabelBackgroundColor = None");
                    streamWriter.WriteLine("LabelColor = White");
                    streamWriter.WriteLine("LabelSize = 14");
                    streamWriter.WriteLine("DrawNumbers = True");
                    streamWriter.WriteLine("ButtonType = Black Square");
                    streamWriter.WriteLine("RotaryType = Gray");
                    streamWriter.WriteLine("LabelPos = Top");
                    streamWriter.WriteLine("DrawToggleIndicators = True");
                    streamWriter.WriteLine("DXSendType = Pulsed");
                    streamWriter.WriteLine("ToggleAsButton = False");
                    streamWriter.WriteLine("AdjustmentSpeed = 100");
                }
            }
        }
        catch (Exception ex)
        {
            OnPluginStatusChanged(Loupedeck.PluginStatus.Error, ex.Message);
        }
    }

    public override void Unload()
    {
    }

    private void OnApplicationStarted(object sender, EventArgs e)
    {
    }

    private void OnApplicationStopped(object sender, EventArgs e)
    {
    }

    public override void RunCommand(string commandName, string parameter)
    {
    }

    public override void ApplyAdjustment(string adjustmentName, string parameter, int diff)
    {
    }

    public static CommandInfoType GetCommandInfo(string parameter)
    {
        var commandInfo = new CommandInfoType { 
            Value = -1, 
            Label = "", 
            LabelBackgroundColor = DefaultLabelBackgroundColor, 
            LabelColor = DefaultLabelColor,
            LabelSize = DefaultLabelSize,
            LabelPos = DefaultLabelPos,
            DrawNumbers = DefaultDrawNumbers,
            DrawToggleIndicators = DefaultDrawToggleIndicators,
            ButtonPath = DefaultButtonPath,
            RotaryPath = DefaultRotaryPath,
            DXSendType = DefaultDXSendType,
            ToggleAsButton = DefaultToggleAsButton
        };
            
        var strArray1 = parameter != null ? parameter.Split(";") : new string[1] { "" };
            
        foreach (var t in strArray1)
        {
            var strArray2 = t.Split("=");
            var lower1 = strArray2[0].Trim().ToLower();
            if (strArray2.Length == 1)
            {
                if (int.TryParse(lower1, out var result1))
                {
                    commandInfo.Value = result1;
                }
                else
                {
                    switch (lower1)
                    {
                        case "c":
                        case "center":
                            commandInfo.Value = 0;
                            continue;
                        case "d":
                        case "down":
                            commandInfo.Value = 2;
                            commandInfo.ButtonPath = EmbeddedResources.FindFile("BlackSquareDownButton.png");
                            continue;
                        case "f":
                        case "fast":
                            commandInfo.Value = 200;
                            continue;
                        case "l":
                        case "left":
                            commandInfo.Value = 3;
                            commandInfo.ButtonPath = EmbeddedResources.FindFile("BlackSquareLeftButton.png");
                            continue;
                        case "max":
                            commandInfo.Value = 100;
                            continue;
                        case "min":
                            commandInfo.Value = -100;
                            continue;
                        case "n":
                        case "norm":
                        case "normal":
                            commandInfo.Value = 100;
                            continue;
                        case "r":
                        case "right":
                            commandInfo.Value = 1;
                            commandInfo.ButtonPath = EmbeddedResources.FindFile("BlackSquareRightButton.png");
                            continue;
                        case "s":
                        case "slow":
                            commandInfo.Value = 25;
                            continue;
                        case "u":
                        case "up":
                            commandInfo.Value = 0;
                            commandInfo.ButtonPath = EmbeddedResources.FindFile("BlackSquareUpButton.png");
                            continue;
                        default:
                            commandInfo.Value = -1;
                            PluginWarning = "Invalid Value";
                            PluginWarningStopwatch.Restart();
                            continue;
                    }
                }
            }
            else if (strArray2.Length == 2)
            {
                var str = strArray2[1].Trim();
                var lower2 = str.ToLower();
                switch (lower1)
                {
                    case "vjoyid":
                    case "defaultvalue":
                    case "upbutton":
                    case "downbutton": 
                    case "axis":
                        continue;
                    case "bt":
                    case "buttontype":
                        switch (lower2)
                        {
                            case "black round":
                            case "br":
                            case "bround":
                                commandInfo.ButtonPath = EmbeddedResources.FindFile("BlackRoundButton.png");
                                continue;
                            case "black square":
                            case "bs":
                            case "bsquare":
                                commandInfo.ButtonPath = EmbeddedResources.FindFile("BlackSquareButton.png");
                                continue;
                            case "ccw":
                            case "counterclockwise":
                                commandInfo.ButtonPath = EmbeddedResources.FindFile("BlackSquareCCWButton.png");
                                continue;
                            case "clockwise":
                            case "cw":
                                commandInfo.ButtonPath = EmbeddedResources.FindFile("BlackSquareCWButton.png");
                                continue;
                            case "d":
                            case "down":
                                commandInfo.ButtonPath = EmbeddedResources.FindFile("BlackSquareDownButton.png");
                                continue;
                            case "gr":
                            case "gray round":
                            case "ground":
                                commandInfo.ButtonPath = EmbeddedResources.FindFile("GrayRoundButton.png");
                                continue;
                            case "l":
                            case "left":
                                commandInfo.ButtonPath = EmbeddedResources.FindFile("BlackSquareLeftButton.png");
                                continue;
                            case "r":
                            case "right":
                                commandInfo.ButtonPath = EmbeddedResources.FindFile("BlackSquareRightButton.png");
                                continue;
                            case "red round":
                            case "rr":
                            case "rround":
                                commandInfo.ButtonPath = EmbeddedResources.FindFile("RedRoundButton.png");
                                continue;
                            case "u":
                            case "up":
                                commandInfo.ButtonPath = EmbeddedResources.FindFile("BlackSquareUpButton.png");
                                continue;
                            default:
                                commandInfo.ButtonPath = EmbeddedResources.FindFile("GraySquareButton.png");
                                continue;
                        }
                    case "dn":
                    case "drawnumbers":
                        switch (lower2)
                        {
                            case "true":
                            case "on":
                            case "yes":
                                commandInfo.DrawNumbers = true;
                                continue;
                            case "false":
                            case "off":
                            case "no":
                                commandInfo.DrawNumbers = false;
                                continue;
                            default:
                                continue;
                        }
                    case "drawtoggleindicators":
                    case "dti":
                        switch (lower2)
                        {
                            case "true":
                            case "on":
                            case "yes":
                                commandInfo.DrawToggleIndicators = true;
                                continue;
                            case "false":
                            case "off":
                            case "no":
                                commandInfo.DrawToggleIndicators = false;
                                continue;
                            default:
                                continue;
                        }
                    case "dx":
                    case "dxsendtype":
                    case "dxst":
                        switch (lower2)
                        {
                            case "hold":
                            case "h":
                                commandInfo.DXSendType = 1;
                                continue;
                            default:
                                commandInfo.DXSendType = 0;
                                continue;
                        }
                    case "l":
                    case "label":
                        commandInfo.Label = str;
                        continue;
                    case "labelbackgroundcolor":
                    case "lb":
                    case "lbc":
                        switch (lower2)
                        {
                            case "black":
                                commandInfo.LabelBackgroundColor = BitmapColor.Black;
                                continue;
                            case "blue":
                                commandInfo.LabelBackgroundColor = new BitmapColor(50, 50, 200);
                                continue;
                            case "gray":
                                commandInfo.LabelBackgroundColor = new BitmapColor(128, 128, 128);
                                continue;
                            case "green":
                                commandInfo.LabelBackgroundColor = new BitmapColor(50, 200, 50);
                                continue;
                            case "none":
                                commandInfo.LabelBackgroundColor = BitmapColor.Transparent;
                                continue;
                            case "purple":
                                commandInfo.LabelBackgroundColor = new BitmapColor(150, 50, 200);
                                continue;
                            case "red":
                                commandInfo.LabelBackgroundColor = new BitmapColor(200, 50, 50);
                                continue;
                            case "white":
                                commandInfo.LabelBackgroundColor = BitmapColor.White;
                                continue;
                            default:
                                if (!uint.TryParse(lower2, out var result2))
                                {
                                    PluginWarning = "Invalid Color";
                                    PluginWarningStopwatch.Restart();
                                    continue;
                                }

                                commandInfo.LabelBackgroundColor = new BitmapColor(result2);
                                continue;
                        }
                    case "labelcolor":
                    case "lc":
                        switch (lower2)
                        {
                            case "black":
                                commandInfo.LabelColor = BitmapColor.Black;
                                continue;
                            case "blue":
                                commandInfo.LabelColor = new BitmapColor(50, 50, 200);
                                continue;
                            case "gray":
                                commandInfo.LabelColor = new BitmapColor(128, 128, 128);
                                continue;
                            case "green":
                                commandInfo.LabelColor = new BitmapColor(50, 200, 50);
                                continue;
                            case "none":
                                commandInfo.LabelColor = BitmapColor.Transparent;
                                continue;
                            case "purple":
                                commandInfo.LabelColor = new BitmapColor(150, 50, 200);
                                continue;
                            case "red":
                                commandInfo.LabelColor = new BitmapColor(200, 50, 50);
                                continue;
                            case "white":
                                commandInfo.LabelColor = BitmapColor.White;
                                continue;
                            default:
                                if (!uint.TryParse(lower2, out var result3))
                                {
                                    PluginWarning = "Invalid Color";
                                    PluginWarningStopwatch.Restart();
                                    continue;
                                }

                                commandInfo.LabelColor = new BitmapColor(result3);
                                continue;
                        }
                    case "labelpos":
                    case "lp":
                        switch (lower2)
                        {
                            case "top":
                            case "t":
                                commandInfo.LabelPos = 7;
                                continue;
                            case "center":
                            case "c":
                                commandInfo.LabelPos = 40;
                                continue;
                            case "bottom":
                            case "b":
                                commandInfo.LabelPos = 73;
                                continue;
                            default:
                                if (!int.TryParse(lower2, out commandInfo.LabelPos))
                                {
                                    PluginWarning = "Invalid Label Position (must be top, center, bottom, or a number)";
                                    PluginWarningStopwatch.Restart();
                                }

                                continue;
                        }
                    case "labelsize":
                    case "ls":
                        if (!int.TryParse(lower2, out var result4))
                        {
                            PluginWarning = "Invalid Label Size (must be a number)";
                            PluginWarningStopwatch.Restart();
                            continue;
                        }

                        commandInfo.LabelSize = result4;
                        continue;
                    case "rotarytype":
                    case "rt":
                        if (!(lower2 == "gray"))
                        {
                            var num = lower2 == "g" ? 1 : 0;
                        }

                        commandInfo.RotaryPath = EmbeddedResources.FindFile("GrayRotary.png");
                        continue;
                    case "tab":
                    case "toggleasbutton":
                        switch (lower2)
                        {
                            case "true":
                            case "on":
                            case "yes":
                                commandInfo.ToggleAsButton = true;
                                continue;
                            case "false":
                            case "off":
                            case "no":
                                commandInfo.ToggleAsButton = false;
                                continue;
                            default:
                                continue;
                        }
                    default:
                        PluginWarning = "Unknown Option";
                        PluginWarningStopwatch.Restart();
                        continue;
                }
            }
            else
            {
                PluginWarning = "Invalid option format. Option format is <option> = <optionvalue>";
                PluginWarningStopwatch.Restart();
                break;
            }
        }

        return commandInfo;
    }
}