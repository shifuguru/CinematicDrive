using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using LemonUI;
using LemonUI.Menus;
using Screen = GTA.UI.Screen;

namespace CinematicDrive
{
    public class LemonMenu : Script
    {
        public static readonly ObjectPool pool = new ObjectPool();
        private static readonly NativeMenu menu = new NativeMenu("Cinematic Drive", "Settings");

        private static readonly NativeCheckboxItem modEnabledToggle = new NativeCheckboxItem("Mod Enabled: ", SettingsManager.ModEnabled);
        private static readonly NativeCheckboxItem debugEnabledToggle = new NativeCheckboxItem("Debug Enabled:", SettingsManager.DebugEnabled);
        private static readonly NativeSliderItem speedItem = new NativeSliderItem("Speed: ", "", 250, SettingsManager.Speed);

        public static List<object> airports = new List<object>()
        {
            "Los Santos Airport",
            "Trevor's Airport",
            "McKenzie Airport"
        };

        private static readonly List<object> styles = new List<object>()
        {
            "Avoid Traffic",
            "Avoid Traffic Extremely",
            "Ignore Lights",
            "Normal",
            "Rushed",
            "Sometimes Overtake Traffic"
        };

        private static readonly NativeListItem<object> drivingStyleItem = new NativeListItem<object>("Driving Style: ", "styles");

        public LemonMenu()
        {
            LoadMenu();
            Tick += OnTick;
            Interval = 1;
        }

        private void OnTick(object sender, EventArgs e)
        {
            pool.Process();
        }

        public static void LoadMenu()
        {
            pool.Add(menu);
            // Add items to the menu:
            menu.Add(modEnabledToggle);
            modEnabledToggle.CheckboxChanged += ToggleMod;
            menu.Add(debugEnabledToggle);
            debugEnabledToggle.CheckboxChanged += ToggleDebug;

            menu.Add(speedItem);
            speedItem.ValueChanged += ChangeDriveSpeed;
            speedItem.Value = SettingsManager.Speed;

            menu.Add(drivingStyleItem);
            // drivingStyleItem.ItemChanged += ChangeDrivingStyle;
            drivingStyleItem.SelectedIndex = styles.IndexOf(SettingsManager.GetDrivingStyleName());
        }
        
        public static void ToggleMenu()
        {
            menu.Visible = !menu.Visible;
        }

        private static void ToggleMod(object sender, EventArgs e)
        {
            SettingsManager.SetModEnabled(modEnabledToggle.Checked);
            Screen.ShowSubtitle($"Cinematic Drive Enabled: {SettingsManager.ModEnabled}", 1500);
            SettingsManager.Save();
        }

        private static void ToggleDebug(object sender, EventArgs e)
        {
            SettingsManager.SetDebugEnabled(debugEnabledToggle.Checked);
            Screen.ShowSubtitle($"Debug Enabled: {SettingsManager.DebugEnabled}", 1500);
            SettingsManager.Save();
        }


        //* Come back to this once Lemon has explained how it works:
        private static void ChangeDriveSpeed(object sender, ItemChangedEventArgs<object> e)
        {
            if (e.Direction == Direction.Left)
            {
                SettingsManager.SetSpeed(SettingsManager.Speed - 1);
            }
            else if (e.Direction == Direction.Right)
            {
                SettingsManager.SetSpeed(SettingsManager.Speed + 1);
            }
        }

        private static void ChangeDrivingStyle(NativeListItem<object> nativeListItem, ItemChangedEventArgs<object> e)
        {
            if (nativeListItem.SelectedItem is string selectedStyle)
            {
                SettingsManager.SetDrivingStyleFromName(selectedStyle);
                Screen.ShowSubtitle($"Driving Style Set: {selectedStyle}", 1500);
                SettingsManager.Save();
            }
        }
        //

        private static void ChangeSpeed(object sender, EventArgs e)
        {
            SettingsManager.SetSpeed(speedItem.Value);
            Screen.ShowSubtitle($"Speed Set: {SettingsManager.Speed}", 1500);
            SettingsManager.Save();
        }

        public static void CreateMenu()
        {
            var enableToggle = new NativeCheckboxItem("Mod Enabled:", SettingsManager.ModEnabled);
            enableToggle.Activated += (sender, e) => { SettingsManager.SetModEnabled(!SettingsManager.ModEnabled); };

            var holdTimeItem = new NativeItem("Hold Time: " + SettingsManager.HoldDurationMs + "ms");
            holdTimeItem.Activated += (sender, e) => { SettingsManager.SetHoldDuration(SettingsManager.HoldDurationMs + 500); };

            menu.Add(enableToggle);
            menu.Add(holdTimeItem);
        }
    }
}
