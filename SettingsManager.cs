using System.Windows.Forms;
using GTA;

namespace CinematicDrive
{
    public class SettingsManager
    {
        public static bool ModEnabled { get; private set; } = true;
        public static bool DebugEnabled { get; private set; } = false; // Set to FALSE before Public Release!!!
        public static bool OnFootEnabled { get; private set; } = false; // Currently not implemented well.
        public static bool EndOnExitEnabled { get; private set; } = true; // End Cinematic on Vehicle Exit
        public static int HoldDurationMs { get; private set; } = 1000;
        public static int Speed { get; private set; } = 20;
        public static DrivingStyle DrivingStyle { get; private set; } = DrivingStyle.Normal;
        public static Keys MenuKey { get; private set; } = Keys.F5;

        public static void LoadSettings()
        {
            ScriptSettings config = ScriptSettings.Load("scripts\\CinematicDrive.ini");

            ModEnabled = config.GetValue("SETTINGS", "Mod Enabled", true);
            HoldDurationMs = config.GetValue("SETTINGS", "HoldDuration", 1000);
            Speed = config.GetValue("SETTINGS", "Speed", 20);
            DrivingStyle = config.GetValue("SETTINGS", "Driving Style", DrivingStyle.Normal);
            MenuKey = config.GetValue("SETTINGS", "MenuKey", Keys.F5);
            OnFootEnabled = config.GetValue("SETTINGS", "On Foot Enabled", false);
            EndOnExitEnabled = config.GetValue("SETTINGS", "End On Exit Enabled", false);

            string drivingStyleName = config.GetValue("SETTINGS", "Driving Style", "Normal");
            SetDrivingStyleFromName(drivingStyleName);

            MenuKey = config.GetValue("SETTINGS", "Menu Key", Keys.F5);
        }
        public static void Save()
        {
            ScriptSettings config = ScriptSettings.Load("scripts\\CinematicDrive.ini");

            config.SetValue("SETTINGS", "ModEnabled", ModEnabled);
            config.SetValue("SETTINGS", "DebugEnabled", DebugEnabled);
            config.SetValue("SETTINGS", "HoldDuration", HoldDurationMs);
            config.SetValue("SETTINGS", "Speed", Speed);
            config.SetValue("SETTINGS", "DrivingStyle", GetDrivingStyleName());
            config.SetValue("SETTINGS", "MenuKey", MenuKey);
            config.SetValue("SETTINGS", "On Foot Enabled", OnFootEnabled);
            config.SetValue("SETTINGS", "End On Exit Enabled", EndOnExitEnabled);

            config.Save();
        }


        public static void SetModEnabled(bool isEnabled) => ModEnabled = isEnabled;
        public static void SetDebugEnabled(bool isEnabled) => DebugEnabled = isEnabled;
        public static void SetOnFootEnabled(bool isEnabled) => OnFootEnabled = isEnabled;
        public static void SetEndOnExitEnabled(bool isEnabled) => EndOnExitEnabled = isEnabled;

        public static void SetHoldDuration(int holdDuration) => HoldDurationMs = holdDuration;
        public static void SetSpeed(int speed) => Speed = speed;
        public static void SetDrivingStyleFromName(string styleName)
        {
            switch (styleName)
            {
                case "Avoid Traffic":
                    DrivingStyle = DrivingStyle.AvoidTraffic;
                    break;
                case "Avoid Traffic Extermely":
                    DrivingStyle = DrivingStyle.AvoidTrafficExtremely;
                    break;
                case "Ignore Lights":
                    DrivingStyle = DrivingStyle.IgnoreLights;
                    break;
                case "Rushed":
                    DrivingStyle = DrivingStyle.Rushed;
                    break;
                case "Sometime Overtake Traffic":
                    DrivingStyle = DrivingStyle.SometimesOvertakeTraffic;
                    break;
                default:
                    DrivingStyle = DrivingStyle.Normal;
                    break;
            }
        }

        public static string GetDrivingStyleName()
        {
            switch (DrivingStyle)
            {
                case DrivingStyle.AvoidTraffic:
                    return "Avoid Traffic";
                case DrivingStyle.AvoidTrafficExtremely:
                    return "Avoid Traffic Extermely";
                case DrivingStyle.IgnoreLights:
                    return "Ignore Lights";
                case DrivingStyle.Rushed:
                    return  "Rushed";
                case DrivingStyle.SometimesOvertakeTraffic:
                    return "Sometime Overtake Traffic";
                default:
                    return "Normal";
            };
        }
    }
}
