namespace Loupedeck.GameControlPlugin;

using System;
using System.Collections.Generic;

using CoreDX.vJoy.Wrapper;

public static class JoystickManager
{
    private static Plugin _plugin;
    private static uint _defaultJoystickId; 
    private static readonly VJoyControllerManager VJoy = VJoyControllerManager.GetManager();
    private static readonly object Lock = new();
    private static readonly Dictionary<uint, Joystick> Joysticks = new ();
    private static readonly Dictionary<int, uint> JoystickIdHashMap = new ();

    public static void Dispose()
    {
        VJoy.Dispose();
    }
    
    public static int ButtonPressDelay { get; } = 50;
    public static void SetDefaultJoystickId(uint id) => _defaultJoystickId = id;

    public static void Initialise(Plugin plugin) => _plugin = plugin;

    public static Joystick GetJoystick(string actionParameter)
    {
        var idHash = actionParameter.GetHashCode();

        if (!JoystickIdHashMap.TryGetValue(idHash, out var id))
        {
            foreach(var settings in actionParameter.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var value = settings.Split('=');

                if (value.Length == 2 && string.Compare(value[0].Trim(), "vjoyid", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    id = uint.Parse(value[1]);
                    break;
                }
            }
                
            JoystickIdHashMap.Add(idHash, id);
        }
            
        if (id == 0)
            id = _defaultJoystickId;
            
        if (!Joysticks.TryGetValue(id, out var joystick))
        {
            lock (Lock)
            {
                if (!Joysticks.TryGetValue(id, out joystick))
                {
                    joystick = MakeJoystick(id);

                    if(joystick is not null)
                        Joysticks.Add(id, joystick);
                }
            }
        }

        return joystick;
    }
    
    private static Joystick MakeJoystick(uint id)
    {
        var vjdStatus = (VjdStat)VJoy.GetVJDStatus(id);
            
        switch (vjdStatus)
        {
            case VjdStat.VJD_STAT_OWN:
                _plugin.OnPluginStatusChanged(PluginStatus.Error, "vJoy Device is already owned by this feeder");
                goto case VjdStat.VJD_STAT_FREE;
            case VjdStat.VJD_STAT_FREE:
                var joystick = new Joystick(VJoy.AcquireController(id)) 
                { 
                    ButtonCount = VJoy.GetVJDButtonNumber(id), 
                    ContPovNumber = VJoy.GetVJDContPovNumber(id), 
                    DiscPovNumber = VJoy.GetVJDDiscPovNumber(id) 
                };

                if (joystick.Controller is null)
                {
                    _plugin.OnPluginStatusChanged(PluginStatus.Error, "Failed to acquire vJoy device");
                    return null;
                }
                
                joystick.Controller.Reset();
                    
                for (uint nBtn = 0; nBtn < joystick.ButtonCount; ++nBtn)
                    joystick.SetBtn(false, nBtn);
                    
                return joystick;
            case VjdStat.VJD_STAT_BUSY:
                _plugin.OnPluginStatusChanged(PluginStatus.Error, "vJoy Device is already owned by another feeder. Cannot continue");
                break;
            case VjdStat.VJD_STAT_MISS:
                _plugin.OnPluginStatusChanged(PluginStatus.Error, "vJoy Device is not installed or disabled.Cannot continue");
                break;
            default:
                _plugin.OnPluginStatusChanged(PluginStatus.Error, "vJoy Device general error. Cannot continue");
                break;
        }

        return null;
    }

    public static int? GetAxisDefaultValue(string actionParameter)
    {
        foreach(var p in actionParameter.Split(";", StringSplitOptions.RemoveEmptyEntries))
        {
            var values = p.Split("=");

            if (values.Length == 2 && values[0].Trim().ToLower() == "defaultvalue")
            {
                return int.Parse(values[1]);
            }
        }

        return null;
    }
}

public class Joystick(IVJoyController joy)
{
    public IVJoyController Controller => joy;

    public uint Id => joy.Id;
    public int MaxValue { get; } = (int?)joy.AxisMaxValue ?? int.MaxValue;
    public int ButtonCount { get; set; }

    public int X { get; set; } = int.MinValue;
    public int Y { get; set; } = int.MinValue;
    public int Z { get; set; } = int.MinValue;
    public int RX { get; set; } = int.MinValue;
    public int RY { get; set; } = int.MinValue;
    public int RZ { get; set; } = int.MinValue;
    public int SL0 { get; set; } = int.MinValue;
    public int SL1 { get; set; } = int.MinValue;
        
    public int ContPovNumber { get; set; }
    public int DiscPovNumber { get; set; }
    public void SetAxis(int value, VJoyControllerManager.USAGES hidUsage)
    {
        switch (hidUsage)
        {
            case VJoyControllerManager.USAGES.X: joy.SetAxisX(value); break;
            case VJoyControllerManager.USAGES.Y: joy.SetAxisY(value); break;
            case VJoyControllerManager.USAGES.Z: joy.SetAxisZ(value); break;
            case VJoyControllerManager.USAGES.Rx: joy.SetAxisRx(value); break;
            case VJoyControllerManager.USAGES.Ry: joy.SetAxisRy(value); break;
            case VJoyControllerManager.USAGES.Rz: joy.SetAxisRz(value); break;
            case VJoyControllerManager.USAGES.Slider0: joy.SetSlider0(value); break;
            case VJoyControllerManager.USAGES.Slider1: joy.SetSlider1(value); break;
            case VJoyControllerManager.USAGES.Wheel: joy.SetWheel(value); break;
            default:
                throw new ArgumentOutOfRangeException(nameof(hidUsage), hidUsage, null);
        }
    }

    public void SetBtn(bool press, uint commandInfoValue)
    {
        if(press)
            joy.PressButton(commandInfoValue);
        else
            joy.ReleaseButton(commandInfoValue);
    }

    public void SetDiscPov(int commandInfoValue, uint pov)
    {
        joy.SetDiscPov(commandInfoValue, pov);
    }

    public static string GetCompatibleAxisName(string axisName) =>
        axisName switch
        {
            "SL0" => "Slider0",
            "Sl1" => "Slider1",
            "RX" => "Rx",
            "RY" => "Ry",
            "RZ" => "Rz",
            _ => axisName
        };
}