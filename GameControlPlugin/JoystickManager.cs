namespace Loupedeck.GameControlPlugin;

using System;
using System.Collections.Generic;

using vJoyInterfaceWrap;

public static class JoystickManager
{
    private static uint _defaultJoystickId; 
    private static Plugin _plugin;
    private static readonly vJoy _vJoy = new();
    private static readonly object _lock = new();
    public static readonly IDictionary<uint, Joystick> Joysticks = new Dictionary<uint, Joystick>();
    public static readonly IDictionary<int, uint> JoystickIdHashMap = new Dictionary<int, uint>();

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
            lock (_lock)
            {
                if (!Joysticks.TryGetValue(id, out joystick))
                {
                    joystick = MakeJoystick(id, actionParameter);

                    Joysticks.Add(id, joystick);
                }
            }
        }

        return joystick;
    }

    private static Joystick MakeJoystick(uint id, string actionParameter)
    {
        var vjdStatus = _vJoy.GetVJDStatus(id);
            
        switch (vjdStatus)
        {
            case VjdStat.VJD_STAT_OWN:
                _plugin.OnPluginStatusChanged(PluginStatus.Error, "vJoy Device is already owned by this feeder");
                goto case VjdStat.VJD_STAT_FREE;
            case VjdStat.VJD_STAT_FREE:
                _vJoy.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X);
                _vJoy.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Y);
                _vJoy.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Z);
                _vJoy.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RX);
                _vJoy.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RY);
                _vJoy.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RZ);
                _vJoy.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_SL0);
                _vJoy.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_SL1);

                var joystick = new Joystick(_vJoy, id) 
                { 
                    ButtonCount = _vJoy.GetVJDButtonNumber(id), 
                    ContPovNumber = _vJoy.GetVJDContPovNumber(id), 
                    DiscPovNumber = _vJoy.GetVJDDiscPovNumber(id) 
                };
                    
                uint DllVer = 0;
                uint DrvVer = 0;
                if (!_vJoy.DriverMatch(ref DllVer, ref DrvVer))
                    _plugin.OnPluginStatusChanged(PluginStatus.Error, "Version of Driver does NOT match DLL Version.");
                switch (vjdStatus)
                {
                    case VjdStat.VJD_STAT_OWN:
                        _plugin.OnPluginStatusChanged(PluginStatus.Error, "Failed to acquire vJoy device");
                        return null;
                    case VjdStat.VJD_STAT_FREE:
                        if (_vJoy.AcquireVJD(id))
                            break;
                        goto case VjdStat.VJD_STAT_OWN;
                }

                long maxValue = 0;
                    
                _vJoy.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref maxValue);
                joystick.MaxValue = (int)maxValue;
                    
                _vJoy.ResetVJD(id);
                    
                for (uint nBtn = 0; nBtn < joystick.ButtonCount; ++nBtn)
                    _vJoy.SetBtn(false, id, nBtn);
                    
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

public class Joystick(vJoy joy, uint id)
{
    public uint Id { get; } = id;
    public int MaxValue { get; set; }
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
    public void SetAxis(int value, HID_USAGES hidUsage)
    {
        joy.SetAxis(value, Id, hidUsage);
    }

    public void SetBtn(bool value, uint commandInfoValue)
    {
        joy.SetBtn(value, Id, commandInfoValue);
    }

    public void SetDiscPov(int commandInfoValue, uint pov)
    {
        joy.SetDiscPov(commandInfoValue, Id, pov);
    }
}