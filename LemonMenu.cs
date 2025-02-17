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
        public static readonly NativeMenu menu = new NativeMenu("Cinematic Drive", "Settings");

        private static readonly NativeCheckboxItem ModEnabledToggle = new NativeCheckboxItem("Mod Enabled: ", SettingsManager.ModEnabled);
        private static readonly NativeCheckboxItem DebugEnabledToggle = new NativeCheckboxItem("Debug Enabled:", SettingsManager.DebugEnabled);
        private static readonly NativeCheckboxItem FirstTimeToggle = new NativeCheckboxItem("First Time: ", SettingsManager.FirstTime);
        private static readonly NativeDynamicItem<int> SpeedItem = new NativeDynamicItem<int>("Speed: ", "Sets the maximum Travel Speed", SettingsManager.Speed);
        private static readonly NativeDynamicItem<string> DrivingStyleItem = new NativeDynamicItem<string>("Driving Style: ", "", SettingsManager.GetDrivingStyleName());

        public static List<object> airports = new List<object>()
        {
            "Los Santos Airport",
            "Trevor's Airport",
            "McKenzie Airport"
        };

        private static readonly List<string> styles = new List<string>()
        {
            "Avoid Traffic",
            "Avoid Traffic Extremely",
            "Ignore Lights",
            "Normal",
            "Rushed",
            "Sometimes Overtake Traffic"
        };

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
            menu.Add(ModEnabledToggle);
            ModEnabledToggle.CheckboxChanged += ToggleMod;
            menu.Add(DebugEnabledToggle);
            DebugEnabledToggle.CheckboxChanged += ToggleDebug;
            menu.Add(FirstTimeToggle);
            FirstTimeToggle.CheckboxChanged += ToggleFirstTime;

            menu.Add(SpeedItem);
            SpeedItem.ItemChanged += ChangeDriveSpeed;
            SpeedItem.SelectedItem = SettingsManager.Speed;

            menu.Add(DrivingStyleItem);
            DrivingStyleItem.ItemChanged += ChangeDrivingStyle;
            DrivingStyleItem.SelectedItem = SettingsManager.GetDrivingStyleName();
        }
        
        public static void ToggleMenu()
        {
            menu.Visible = !menu.Visible;
        }

        private static void ToggleMod(object sender, EventArgs e)
        {
            SettingsManager.SetModEnabled(ModEnabledToggle.Checked);
            Screen.ShowSubtitle($"Cinematic Drive Enabled: {SettingsManager.ModEnabled}", 1500);
            SettingsManager.Save();
        }

        private static void ToggleDebug(object sender, EventArgs e)
        {
            SettingsManager.SetDebugEnabled(DebugEnabledToggle.Checked);
            Screen.ShowSubtitle($"Debug Enabled: {SettingsManager.DebugEnabled}", 1500);
            SettingsManager.Save();
        }

        private static void ToggleFirstTime(object sender, EventArgs e)
        {
            SettingsManager.SetFirstTime(FirstTimeToggle.Checked);
            Screen.ShowSubtitle($"First Time: {SettingsManager.FirstTime}", 1500);
            SettingsManager.Save();
        }

        private static void ChangeDriveSpeed(object sender, ItemChangedEventArgs<int> e)
        {
            SpeedItem.SelectedItem = SettingsManager.Speed;
            int max = 250;
            int min = 0;

            int increment = e.Direction == Direction.Left ? -1 : 1;

            e.Object = (e.Object + increment - min + (max - min + 1)) % (max - min + 1) + min;
            SettingsManager.SetSpeed(e.Object);
            Screen.ShowSubtitle($"Driving Speed Set: {e.Object}", 1500);
            SettingsManager.Save();
        }

        private static void ChangeDrivingStyle(object sender, ItemChangedEventArgs<string> e)
        {
            int currentIndex = styles.IndexOf(DrivingStyleItem.SelectedItem);

            if (currentIndex == -1) return;

            int increment = e.Direction == Direction.Left? -1 : 1;
            int newIndex = (currentIndex + increment + styles.Count) % styles.Count;
            string selectedStyle = styles[newIndex];

            Screen.ShowSubtitle($"DEBUG: e.Object={e.Object}, SelectedItem={DrivingStyleItem.SelectedItem}, currentIndex={currentIndex}, newIndex={newIndex}", 2500);

            e.Object = selectedStyle;
            SettingsManager.SetDrivingStyleFromName(selectedStyle);
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
